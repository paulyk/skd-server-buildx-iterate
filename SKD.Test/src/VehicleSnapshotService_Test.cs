using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class VehicleSnapshotServiceTest : TestBase {

        private SkdContext ctx;
        public VehicleSnapshotServiceTest() {
            ctx = GetAppDbContext();
        }

        [Fact]
        public async Task can_generate_snapshot() {
            // setup
            var snapshotInput = await Gen_Test_Data_For_Vehicle_Snapshot();

            var service = new VehicleSnapshotService(ctx);
            snapshotInput.RunDate = DateTime.Now.Date;
            var payload = await service.GenerateSnapshot(snapshotInput);

            Assert.Equal(false, payload.Entity.WasGenerated);

            // custom received
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            var vehicle = ctx.Vehicles.OrderBy(t => t.KitNo).First();
            await AddVehicleTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, vehicle.KitNo, "", DateTime.Now.Date);
            payload = await service.GenerateSnapshot(snapshotInput);
            var snapshots_count = ctx.VehicleSnapshots.Count();
            Assert.Equal(1, snapshots_count);
        }

        [Fact]
        public async Task cannot_generate_snapshot_with_same_run_date() {
            // setup
            var snapshotInput = await Gen_Test_Data_For_Vehicle_Snapshot();

            var vehicle = ctx.Vehicles.OrderBy(t => t.KitNo).First();
            await AddVehicleTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, vehicle.KitNo, "", DateTime.Now.Date);
            var service = new VehicleSnapshotService(ctx);

            snapshotInput.RunDate = new DateTime(2020, 11, 24);
            var payload = await service.GenerateSnapshot(snapshotInput);
            var snapshots_count = ctx.VehicleSnapshots.Count();
            Assert.Equal(1, snapshots_count);

            // test with same runDate
            payload = await service.GenerateSnapshot(snapshotInput);

            var errorCount = payload.Errors.Count;
            Assert.Equal(1, errorCount);

        }

                [Fact]
        public async Task generate_snapshot_with_same_plant_code_increments_sequence() {
            // setup plant 1
            var plantCode_1 = Gen_PlantCode();
            var snapshotInput_1 = await Gen_Test_Data_For_Vehicle_Snapshot(plantCode_1);

            var vehicle_1 = ctx.Vehicles.Where(t => t.Lot.Plant.Code == plantCode_1).OrderBy(t => t.KitNo).First();
            await AddVehicleTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, vehicle_1.KitNo, "", DateTime.Now.Date);
            var service = new VehicleSnapshotService(ctx);

            snapshotInput_1.RunDate = new DateTime(2020, 11, 24);
            var payload = await service.GenerateSnapshot(snapshotInput_1);
            Assert.Equal(1, payload.Entity.Sequence);

            snapshotInput_1.RunDate = new DateTime(2020, 11, 25);
            payload = await service.GenerateSnapshot(snapshotInput_1);
            Assert.Equal(2, payload.Entity.Sequence);


            // setup plant 2
            var plantCode_2 = Gen_PlantCode();
            var snapshotInput_2 = await Gen_Test_Data_For_Vehicle_Snapshot(plantCode_2);

            var vehicle_2 = ctx.Vehicles.Where(t => t.Lot.Plant.Code == plantCode_2).OrderBy(t => t.KitNo).First();
            await AddVehicleTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, vehicle_2.KitNo, "", DateTime.Now.Date);

            snapshotInput_2.RunDate = new DateTime(2020, 12, 1);
            payload = await service.GenerateSnapshot(snapshotInput_2);
            Assert.Equal(1, payload.Entity.Sequence);

            snapshotInput_2.RunDate = new DateTime(2020, 12, 2);
            payload = await service.GenerateSnapshot(snapshotInput_2);
            Assert.Equal(2, payload.Entity.Sequence);

            // total snapshot run entries
            var vehicle_snapshot_runs = await ctx.VehicleSnapshotRuns.CountAsync();
            Assert.Equal(4, vehicle_snapshot_runs);
        }

        [Fact]
        public async Task can_create_full_snapshow_timeline() {
            // setup
            var service = new VehicleSnapshotService(ctx);
            var snapshotInput = await Gen_Test_Data_For_Vehicle_Snapshot();
            var vehicle = ctx.Vehicles.OrderBy(t => t.KitNo).First();
            var dealerCode = "D12345678";
            snapshotInput.RunDate = new DateTime(2020, 12, 1);

            // 1.  empty
            await service.GenerateSnapshot(snapshotInput);
            var snapshotPayload = await service.GetSnapshot(snapshotInput.PlantCode, snapshotInput.RunDate);
            Assert.Null(snapshotPayload);

            // 2.  custom received
            await AddVehicleTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, vehicle.KitNo, "", DateTime.Now.Date);
            snapshotInput.RunDate = new DateTime(2020, 11, 24);
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshot(snapshotInput.PlantCode, snapshotInput.RunDate);
            var vehicleStatus = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.CUSTOM_RECEIVED, vehicleStatus.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Added, vehicleStatus.TxType);

            // 2.  custom no change
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);
            var snapshotRecordCount = await ctx.VehicleSnapshots.CountAsync();
            Assert.Equal(2, snapshotRecordCount);
            snapshotPayload = await service.GetSnapshot(snapshotInput.PlantCode, snapshotInput.RunDate);
            vehicleStatus = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(1, snapshotPayload.Entries.Count);
            Assert.Equal(TimeLineEventType.CUSTOM_RECEIVED, vehicleStatus.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.NoChange, vehicleStatus.TxType);

            // 3.  plan build
            await AddVehicleTimelineEntry(TimeLineEventType.PLAN_BUILD, vehicle.KitNo, "", DateTime.Now.Date);
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshot(snapshotInput.PlantCode, snapshotInput.RunDate);
            vehicleStatus = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.PLAN_BUILD, vehicleStatus.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Changed, vehicleStatus.TxType);

            // 4.  no change
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshot(snapshotInput.PlantCode, snapshotInput.RunDate);
            vehicleStatus = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.PLAN_BUILD, vehicleStatus.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.NoChange, vehicleStatus.TxType);

            // 5. build completed
            await AddVehicleTimelineEntry(TimeLineEventType.BULD_COMPLETED, vehicle.KitNo, "", DateTime.Now.Date);
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshot(snapshotInput.PlantCode, snapshotInput.RunDate);
            vehicleStatus = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.BULD_COMPLETED, vehicleStatus.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Changed, vehicleStatus.TxType);

            // 5.  gate release
            await AddVehicleTimelineEntry(TimeLineEventType.GATE_RELEASED, vehicle.KitNo, "", DateTime.Now.Date);
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshot(snapshotInput.PlantCode, snapshotInput.RunDate);
            vehicleStatus = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.GATE_RELEASED, vehicleStatus.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Changed, vehicleStatus.TxType);


            // 6.  wholesale
            var wholesaleDate = snapshotInput.RunDate.Date;

            await AddVehicleTimelineEntry(TimeLineEventType.WHOLE_SALE, vehicle.KitNo, dealerCode, wholesaleDate);
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshot(snapshotInput.PlantCode, snapshotInput.RunDate);
            vehicleStatus = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.WHOLE_SALE, vehicleStatus.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Final, vehicleStatus.TxType);
            Assert.Equal(PartnerStatus_ChangeStatus.Final, vehicleStatus.TxType);
            Assert.Equal(dealerCode, vehicleStatus.DealerCode);

            // 7.  no change ( should still be final)
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(2);
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshot(snapshotInput.PlantCode, snapshotInput.RunDate);
            vehicleStatus = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.WHOLE_SALE, vehicleStatus.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Final, vehicleStatus.TxType);

            // 8.  wholesale
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(4);
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshot(snapshotInput.PlantCode, snapshotInput.RunDate);
            Assert.Null(snapshotPayload);

        }

        [Fact]
        public async Task can_get_vehicle_snapshot_dates() {
            // setup
            var service = new VehicleSnapshotService(ctx);
            var snapshotInput = await Gen_Test_Data_For_Vehicle_Snapshot();
            var vehicle_1 = ctx.Vehicles.OrderBy(t => t.KitNo).First();
            var vehicle_2 = ctx.Vehicles.OrderBy(t => t.KitNo).Skip(1).First();

            // 1. vehicle snapshot run with no entries
            snapshotInput.RunDate = new DateTime(2020, 12, 1);
            await service.GenerateSnapshot(snapshotInput);
            var snapshotPayload = await service.GetSnapshot(snapshotInput.PlantCode, snapshotInput.RunDate);
            Assert.Null(snapshotPayload);
            var snapshotDates = await service.GetSnapshotDates(snapshotInput.PlantCode);
            var snapshotDatesCount = snapshotDates.Count();
            Assert.Equal(0, snapshotDatesCount);

            // 2.  custom received
            await AddVehicleTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, vehicle_1.KitNo, "", DateTime.Now.Date);
            await AddVehicleTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, vehicle_2.KitNo, "", DateTime.Now.Date);
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshot(snapshotInput.PlantCode, snapshotInput.RunDate);
            Assert.Equal(2, snapshotPayload.Entries.Count);
            snapshotDates = await service.GetSnapshotDates(snapshotInput.PlantCode);
            snapshotDatesCount  = snapshotDates.Count;
            Assert.Equal(1, snapshotDatesCount);

            // 3.  no change
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);

            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);

            var totalSnapshotEntries = await ctx.VehicleSnapshots.CountAsync();
            Assert.Equal(6, totalSnapshotEntries);

            snapshotDates = await service.GetSnapshotDates(snapshotInput.PlantCode);
            Assert.Equal(3, snapshotDates.Count);
        }

        #region test helper methods
        private async Task<List<VehicleSnapshotRunDTO.Entry>> GetVehiclePartnerStatusReport(
            string plantCode,
            string engineComponentCode,
            DateTime date) {

            var snapshotInput = new VehicleSnapshotInput {
                PlantCode = plantCode,
                EngineComponentCode = engineComponentCode,
                RunDate = date
            };

            // initial
            var service = new VehicleSnapshotService(ctx);
            var payload = await service.GetSnapshot(snapshotInput.PlantCode, snapshotInput.RunDate);
            return payload.Entries.ToList();
        }

        private async Task AddEngineSerialNumberComponentScan(string kitNo, string engineComponentCode, string engineSerial) {
            var engineVehicleComponent = ctx.VehicleComponents
                .First(t => t.Vehicle.KitNo == kitNo && t.Component.Code == engineComponentCode);
            var scanService = new ComponentScanService(ctx);
            var createScanPayload = await scanService.CreateComponentScan(new ComponentScanInput {
                VehicleComponentId = engineVehicleComponent.Id,
                Scan1 = engineSerial,
                Scan2 = ""
            });

            var componentScan = createScanPayload.Entity;
            var dcwsService = new DCWSResponseService(ctx);
            await dcwsService.CreateDCWSResponse(new DCWWResponseInput {
                ComponentScanId = componentScan.Id,
                ResponseCode = "NONE",
                ErrorMessage = ""
            });
        }
        private async Task AddVehicleTimelineEntry(TimeLineEventType eventType, string kitNo, string eventNote, DateTime eventDate) {
            var service = new VehicleService(ctx);
            var payload = await service.CreateVehicleTimelineEvent(new VehicleTimelineEventInput {
                KitNo = kitNo,
                EventType = eventType,
                EventNote = eventNote,
                EventDate = eventDate
            });
        }

        private async Task<VehicleSnapshotInput> Gen_Test_Data_For_Vehicle_Snapshot(string plantCode = null) {
            var input = new VehicleSnapshotInput {
                RunDate = DateTime.Now.Date,
                PlantCode = plantCode != null ? plantCode : Gen_PlantCode(),
                EngineComponentCode = "EN"
            };

            Gen_VehicleTimelineEventTypes(ctx);

            var lotNo = Gen_LotNo();
            var modelCode = Gen_VehicleModel_Code();
            var vehicleLot = await Gen_Vehicle_Lot(
                ctx, lotNo, input.PlantCode,
                modelCode, new List<(string, string)> { (input.EngineComponentCode, "stating_1"), ("AB", "station_2") });

            // assert setup correct
            var vehicles = await ctx.Vehicles.AsNoTracking().ToListAsync();
            var vehicle_count = vehicles.Count();

            return input;
        }

        public async Task<VehicleLot> Gen_Vehicle_Lot(
            SkdContext ctx,
            string lotNo,
            string plantCode,
            string modelCode,
            List<(string, string)> component_stations
        ) {
            var model = Gen_VehicleModel(
                ctx,
                modelCode,
                component_stations
            );
            var kitNos = Enumerable.Range(1, 6).Select(n => Gen_KitNo()).ToList();

            var input = Gen_VehicleLot_Input(lotNo, plantCode, modelCode, kitNos);

            var service = new VehicleService(ctx);
            var payload = await service.CreateVehicleLot(input);

            return payload.Entity;
        }
        #endregion

    }
}