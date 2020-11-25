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

            var snapshots_count = ctx.VehicleStatusSnapshots.Count();
            Assert.Equal(0, snapshots_count);

            // custom received
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            var vehicle = ctx.Vehicles.OrderBy(t => t.KitNo).First();
            await AddVehicleTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, vehicle.KitNo, "", DateTime.Now.Date);
            payload = await service.GenerateSnapshot(snapshotInput);

            snapshots_count = ctx.VehicleStatusSnapshots.Count();
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
            var snapshots_count = ctx.VehicleStatusSnapshots.Count();
            Assert.Equal(1, snapshots_count);

            // test with same runDate
            payload = await service.GenerateSnapshot(snapshotInput);

            var errorCount = payload.Errors.Count;
            Assert.Equal(1, errorCount);

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
            var snapshotPayload = await service.GetSnapshot(snapshotInput);
            var entryCount = snapshotPayload.Entries.Count;
            Assert.Equal(0, entryCount);

            // 2.  custom received
            await AddVehicleTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, vehicle.KitNo, "", DateTime.Now.Date);
            snapshotInput.RunDate = new DateTime(2020, 11, 24);
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshot(snapshotInput);
            var vehicleStatus = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.CUSTOM_RECEIVED, vehicleStatus.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Added, vehicleStatus.TxType);

            // 2.  custom no change
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);
            var snapshotRecordCount = await ctx.VehicleStatusSnapshots.CountAsync();
            Assert.Equal(2, snapshotRecordCount);
            snapshotPayload = await service.GetSnapshot(snapshotInput);
            vehicleStatus = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(1, snapshotPayload.Entries.Count);
            Assert.Equal(TimeLineEventType.CUSTOM_RECEIVED, vehicleStatus.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.NoChange, vehicleStatus.TxType);

            // 3.  plan build
            await AddVehicleTimelineEntry(TimeLineEventType.PLAN_BUILD, vehicle.KitNo, "", DateTime.Now.Date);
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshot(snapshotInput);
            vehicleStatus = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.PLAN_BUILD, vehicleStatus.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Changed, vehicleStatus.TxType);

            // 4.  no change
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshot(snapshotInput);
            vehicleStatus = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.PLAN_BUILD, vehicleStatus.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.NoChange, vehicleStatus.TxType);

            // 5. build completed
            await AddVehicleTimelineEntry(TimeLineEventType.BULD_COMPLETED, vehicle.KitNo, "", DateTime.Now.Date);
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshot(snapshotInput);
            vehicleStatus = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.BULD_COMPLETED, vehicleStatus.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Changed, vehicleStatus.TxType);

            // 5.  gate release
            await AddVehicleTimelineEntry(TimeLineEventType.GATE_RELEASED, vehicle.KitNo, "", DateTime.Now.Date);
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshot(snapshotInput);
            vehicleStatus = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.GATE_RELEASED, vehicleStatus.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Changed, vehicleStatus.TxType);


            // 6.  wholesale
            var wholesaleDate = snapshotInput.RunDate.Date;

            await AddVehicleTimelineEntry(TimeLineEventType.WHOLE_SALE, vehicle.KitNo, dealerCode, wholesaleDate);
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshot(snapshotInput);
            vehicleStatus = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.WHOLE_SALE, vehicleStatus.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Final, vehicleStatus.TxType);
            Assert.Equal(PartnerStatus_ChangeStatus.Final, vehicleStatus.TxType);
            Assert.Equal(dealerCode, vehicleStatus.DealerCode);

            // 7.  no change ( should still be final)
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(2);
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshot(snapshotInput);
            vehicleStatus = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.WHOLE_SALE, vehicleStatus.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Final, vehicleStatus.TxType);

            // 8.  wholesale
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(4);
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshot(snapshotInput);
            vehicleStatus = snapshotPayload.Entries.FirstOrDefault(t => t.KitNo == vehicle.KitNo);
            Assert.Null(vehicleStatus);

        }


        /*
        public async Task can_generate_partner_status() {
            // setup

            var plantCode = Gen_PlantCode();
            var engineComponentCode = "EN";
            var engineSerial = "SN777777";
            var dealerCode = "DLR77777";
            // generate vehicle lot with 6 kits (vehicles)
            await Gen_PartStatus_Test_Data(plantCode, engineComponentCode);

            // services
            var service = new PartnerStatusService(ctx);
            var vehicleService = new VehicleService(ctx);

            // day 1  (no timeline events)
            var date = new DateTime(2020, 12, 1);
            var vehicleEntries = await GetVehiclePartnerStatusReport(plantCode, engineComponentCode, date);
            var entryCount = vehicleEntries.Count;
            Assert.Equal(0, entryCount);

            // day 2  (Custom Received)
            date = date.AddDays(1);
            var vehicle = ctx.Vehicles.OrderBy(t => t.KitNo).First();
            await AddVehicleTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, vehicle.KitNo, "", date, date);
            vehicleEntries = await GetVehiclePartnerStatusReport(plantCode, engineComponentCode, date);

            entryCount = vehicleEntries.Count;
            Assert.Equal(1, entryCount);
            var entry = vehicleEntries.First();
            Assert.Equal(PartnerStatus_ChangeStatus.Added, entry.TxType);
            Assert.Equal(PartnerStatus_TimelineStatusType.CustomReceived, entry.CurrentStatusType);

            // day 3   (no change)
            date = date.AddDays(1);
            vehicleEntries = await GetVehiclePartnerStatusReport(plantCode, engineComponentCode, date);
            entryCount = vehicleEntries.Count;
            Assert.Equal(1, entryCount);
            entry = vehicleEntries.First();
            Assert.Equal(PartnerStatus_ChangeStatus.NoChange, entry.TxType);
            Assert.Equal(PartnerStatus_TimelineStatusType.CustomReceived, entry.CurrentStatusType);

            // day 3   (PLan Build)
            date = date.AddDays(1);
            await AddVehicleTimelineEntry(TimeLineEventType.PLAN_BUILD, vehicle.KitNo, "", date, date);
            vehicleEntries = await GetVehiclePartnerStatusReport(plantCode, engineComponentCode, date);
            entry = vehicleEntries.First();
            Assert.Equal(PartnerStatus_ChangeStatus.Changed, entry.TxType);
            Assert.Equal(PartnerStatus_TimelineStatusType.FPBP, entry.CurrentStatusType);

            // day 5   (no change)
            date = date.AddDays(1);
            vehicleEntries = await GetVehiclePartnerStatusReport(plantCode, engineComponentCode, date);
            entry = vehicleEntries.First();
            Assert.Equal(PartnerStatus_ChangeStatus.NoChange, entry.TxType);
            Assert.Equal(PartnerStatus_TimelineStatusType.FPBP, entry.CurrentStatusType);

            // day 6   (Build Completed)
            // Scan engine serial number 
            // DCWS Response accepting engine serial number
            // Add Build Completed timeline event
            date = date.AddDays(1);
            vehicle = ctx.Vehicles.OrderBy(t => t.KitNo).First();
            await AddEngineSerialNumberComponentScan(vehicle.KitNo, engineComponentCode, engineSerial);
            await AddVehicleTimelineEntry(TimeLineEventType.BULD_COMPLETED, vehicle.KitNo, "", date, date);
            vehicleEntries = await GetVehiclePartnerStatusReport(plantCode, engineComponentCode, date);
            entry = vehicleEntries.First();
            Assert.Equal(PartnerStatus_ChangeStatus.Changed, entry.TxType);
            Assert.Equal(PartnerStatus_TimelineStatusType.BuildCompleted, entry.CurrentStatusType);
            Assert.Equal(engineSerial, entry.EngineSerialNumber);

            // day 6   (Gate Release)
            date = date.AddDays(1);
            vehicle = ctx.Vehicles.OrderBy(t => t.KitNo).First();
            await AddVehicleTimelineEntry(TimeLineEventType.GATE_RELEASED, vehicle.KitNo, "", date, date);
            vehicleEntries = await GetVehiclePartnerStatusReport(plantCode, engineComponentCode, date);
            entry = vehicleEntries.First();
            Assert.Equal(PartnerStatus_ChangeStatus.Changed, entry.TxType);
            Assert.Equal(PartnerStatus_TimelineStatusType.GateRelease, entry.CurrentStatusType);

            // day 7  (Wholesale)
            date = date.AddDays(1);
            vehicle = ctx.Vehicles.OrderBy(t => t.KitNo).First();
            await AddVehicleTimelineEntry(TimeLineEventType.WHOLE_SALE, vehicle.KitNo, dealerCode, date, date);
            vehicleEntries = await GetVehiclePartnerStatusReport(plantCode, engineComponentCode, date);
            entry = vehicleEntries.First();
            Assert.Equal(PartnerStatus_ChangeStatus.Final, entry.TxType);
            Assert.Equal(PartnerStatus_TimelineStatusType.Wholesale, entry.CurrentStatusType);
            Assert.Equal(dealerCode, entry.DealerCode);

            // day 8   (final)
            date = date.AddDays(1);
            vehicleEntries = await GetVehiclePartnerStatusReport(plantCode, engineComponentCode, date);
            entry = vehicleEntries.First();
            Assert.Equal(PartnerStatus_ChangeStatus.Final, entry.TxType);
            Assert.Equal(PartnerStatus_TimelineStatusType.Wholesale, entry.CurrentStatusType);

        }
        */

        #region test helper methods
        private async Task<List<VehicleSnapshoDTO.Entry>> GetVehiclePartnerStatusReport(
            string plantCode,
            string engineComponentCode,
            DateTime date) {

            var partnerStatusInput = new VehicleSnapshotInput {
                PlantCode = plantCode,
                EngineComponentCode = engineComponentCode,
                RunDate = date
            };

            // initial
            var service = new VehicleSnapshotService(ctx);
            var payload = await service.GetSnapshot(partnerStatusInput);
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

        private async Task<VehicleSnapshotInput> Gen_Test_Data_For_Vehicle_Snapshot() {
            var input = new VehicleSnapshotInput {
                RunDate = DateTime.Now.Date,
                PlantCode = Gen_PlantCode(),
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