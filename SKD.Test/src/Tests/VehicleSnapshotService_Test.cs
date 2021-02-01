using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class VehicleSnapshotServiceTest : TestBase {

        string engineCode = "EN";
        public VehicleSnapshotServiceTest() {
            ctx = GetAppDbContext();
            Gen_Baseline_Test_Seed_Data(componentCodes: new List<string> { "DA", "PA", engineCode });
        }

        [Fact]
        public async Task can_generate_snapshot() {
            // setup
            var snapshotInput = new KitSnapshotInput {
                RunDate = DateTime.Now.Date,
                PlantCode = ctx.Plants.Select(t => t.Code).First(),
                EngineComponentCode = engineCode
            };

            var service = new KitSnapshotService(ctx);
            var payload = await service.GenerateSnapshot(snapshotInput);

            var seqeunceNumber = payload.Entity.Sequence;
            Assert.Null(seqeunceNumber);

            // custom received
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            var vehicle = ctx.Vehicles.OrderBy(t => t.KitNo).First();
            await AddVehicleTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, vehicle.KitNo, "", DateTime.Now.Date);
            payload = await service.GenerateSnapshot(snapshotInput);
            var snapshots_count = ctx.VehicleSnapshots.Count();
            Assert.Equal(1, snapshots_count);
        }

        [Fact]
        public async Task can_create_full_snapshow_timeline() {
            // setup
            var service = new KitSnapshotService(ctx);
            var vehicle = ctx.Vehicles.OrderBy(t => t.KitNo).First();
            var dealerCode = "D12345678";
            var eventDate = new DateTime(2020, 12, 1);
            var snapshotInput = new KitSnapshotInput {
                RunDate = new DateTime(2020, 12, 2),
                PlantCode = ctx.Plants.Select(t => t.Code).First(),
                EngineComponentCode = engineCode
            };

            // 1.  empty
            await service.GenerateSnapshot(snapshotInput);
            var snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            Assert.Null(snapshotPayload);

            // 2.  custom received
            eventDate = eventDate.AddDays(1);
            await AddVehicleTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, vehicle.KitNo, "", eventDate);
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);

            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            var vehicleSnapshot = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.CUSTOM_RECEIVED, vehicleSnapshot.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Added, vehicleSnapshot.TxType);

            // 2.  custom no change
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);
            var snapshotRecordCount = await ctx.VehicleSnapshots.CountAsync();
            Assert.Equal(2, snapshotRecordCount);
            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            vehicleSnapshot = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(1, snapshotPayload.Entries.Count);
            Assert.Equal(TimeLineEventType.CUSTOM_RECEIVED, vehicleSnapshot.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.NoChange, vehicleSnapshot.TxType);

            // 3.  plan build
            eventDate = eventDate.AddDays(1);
            await AddVehicleTimelineEntry(TimeLineEventType.PLAN_BUILD, vehicle.KitNo, "", eventDate);
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);

            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            vehicleSnapshot = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.PLAN_BUILD, vehicleSnapshot.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Changed, vehicleSnapshot.TxType);

            // 4.  no change
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            vehicleSnapshot = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.PLAN_BUILD, vehicleSnapshot.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.NoChange, vehicleSnapshot.TxType);

            // 5. build completed
            eventDate = eventDate.AddDays(1);
            await AddVehicleTimelineEntry(TimeLineEventType.BULD_COMPLETED, vehicle.KitNo, "", eventDate);
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);

            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            vehicleSnapshot = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.BULD_COMPLETED, vehicleSnapshot.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Changed, vehicleSnapshot.TxType);

            // 5.  gate release
            eventDate = eventDate.AddDays(1);
            await AddVehicleTimelineEntry(TimeLineEventType.GATE_RELEASED, vehicle.KitNo, "", eventDate);
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);

            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            vehicleSnapshot = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.GATE_RELEASED, vehicleSnapshot.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Changed, vehicleSnapshot.TxType);


            // 6.  wholesale
            eventDate = eventDate.AddDays(1);
            var wholesaleDate = eventDate;
            await AddVehicleTimelineEntry(TimeLineEventType.WHOLE_SALE, vehicle.KitNo, dealerCode, eventDate);
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);

            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            vehicleSnapshot = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.WHOLE_SALE, vehicleSnapshot.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Final, vehicleSnapshot.TxType);
            Assert.Equal(PartnerStatus_ChangeStatus.Final, vehicleSnapshot.TxType);
            Assert.Equal(dealerCode, vehicleSnapshot.DealerCode);

            // 7.  no change ( should still be final)
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(2);
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            vehicleSnapshot = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.WHOLE_SALE, vehicleSnapshot.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Final, vehicleSnapshot.TxType);

            // 8.  wholesale
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(4);
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            Assert.Null(snapshotPayload);

        }

        [Fact]
        public async Task creates_original_plan_build_date_only_once() {
            // setup
            var service = new KitSnapshotService(ctx);
            var snapshotInput = new KitSnapshotInput {
                RunDate = new DateTime(2020, 12, 1),
                PlantCode = ctx.Plants.Select(t => t.Code).First(),
                EngineComponentCode = engineCode
            };
            var vehicle = ctx.Vehicles.OrderBy(t => t.KitNo).First();
            var first_plan_build_date = new DateTime(2020, 1, 15);

            // 1.  custom received
            await AddVehicleTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, vehicle.KitNo, "", DateTime.Now.Date);
            snapshotInput.RunDate = new DateTime(2020, 11, 24);
            var payload = await service.GenerateSnapshot(snapshotInput);
            var snapshotRun = await service.GetSnapshotRunBySequence(snapshotInput.PlantCode, payload.Entity.Sequence.Value);
            var vehicleSnapshot = snapshotRun.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(1, snapshotRun.Entries.Count);
            Assert.Equal(TimeLineEventType.CUSTOM_RECEIVED, vehicleSnapshot.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Added, vehicleSnapshot.TxType);

            // 1.  plan build 
            await AddVehicleTimelineEntry(TimeLineEventType.PLAN_BUILD, vehicle.KitNo, "", first_plan_build_date);
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            payload = await service.GenerateSnapshot(snapshotInput);
            snapshotRun = await service.GetSnapshotRunBySequence(snapshotInput.PlantCode, payload.Entity.Sequence.Value);
            vehicleSnapshot = snapshotRun.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(1, snapshotRun.Entries.Count);
            Assert.Equal(TimeLineEventType.PLAN_BUILD, vehicleSnapshot.CurrentTimelineEvent);
            Assert.Equal(first_plan_build_date, vehicleSnapshot.PlanBuild);
            Assert.Equal(first_plan_build_date, vehicleSnapshot.OriginalPlanBuild);
            Assert.Equal(PartnerStatus_ChangeStatus.Changed, vehicleSnapshot.TxType);


            // 1. edit plan build date does not change original
            var new_plan_build_date = first_plan_build_date.AddDays(2);
            await AddVehicleTimelineEntry(TimeLineEventType.PLAN_BUILD, vehicle.KitNo, "", new_plan_build_date);
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            payload = await service.GenerateSnapshot(snapshotInput);
            snapshotRun = await service.GetSnapshotRunBySequence(snapshotInput.PlantCode, payload.Entity.Sequence.Value);
            vehicleSnapshot = snapshotRun.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(1, snapshotRun.Entries.Count);
            Assert.Equal(TimeLineEventType.PLAN_BUILD, vehicleSnapshot.CurrentTimelineEvent);
            Assert.Equal(new_plan_build_date, vehicleSnapshot.PlanBuild);
            Assert.Equal(first_plan_build_date, vehicleSnapshot.OriginalPlanBuild);
            Assert.Equal(PartnerStatus_ChangeStatus.NoChange, vehicleSnapshot.TxType);
        }

        [Fact]
        public async Task vehicle_can_have_multiple_timeline_events_before_first_snapshot() {
            // setup
            var eventDate = new DateTime(2020, 12, 1);
            var snapshotInput = new KitSnapshotInput {
                RunDate = new DateTime(2020, 12, 1),
                PlantCode = ctx.Plants.Select(t => t.Code).First(),
                EngineComponentCode = engineCode
            };

            var vehicle = ctx.Vehicles.OrderBy(t => t.KitNo).First();
            await AddVehicleTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, vehicle.KitNo, "", eventDate);
            await AddVehicleTimelineEntry(TimeLineEventType.PLAN_BUILD, vehicle.KitNo, "", eventDate.AddDays(1));

            var service = new KitSnapshotService(ctx);
            await service.GenerateSnapshot(snapshotInput);

            var snapshots_count = ctx.VehicleSnapshots.Count();
            Assert.Equal(1, snapshots_count);

            var snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            var vehicleSnapshot = snapshotPayload.Entries.First(t => t.KitNo == vehicle.KitNo);
            Assert.Equal(TimeLineEventType.PLAN_BUILD, vehicleSnapshot.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Added, vehicleSnapshot.TxType);
        }

        [Fact]
        public async Task cannot_generate_snapshot_with_same_run_date() {
            // setup

            var vehicle = ctx.Vehicles.OrderBy(t => t.KitNo).First();
            await AddVehicleTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, vehicle.KitNo, "", DateTime.Now.Date);
            var service = new KitSnapshotService(ctx);

            var snapshotInput = new KitSnapshotInput {
                RunDate = new DateTime(2020, 11, 24),
                PlantCode = ctx.Plants.Select(t => t.Code).First(),
                EngineComponentCode = engineCode
            };
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
            var plantCode_1 = ctx.Plants.Select(t => t.Code).First();

            var snapshotInput_1 = new KitSnapshotInput {
                PlantCode = plantCode_1,
                EngineComponentCode = engineCode
            };

            var vehicle_1 = ctx.Vehicles.Where(t => t.Lot.Plant.Code == plantCode_1).OrderBy(t => t.KitNo).First();
            await AddVehicleTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, vehicle_1.KitNo, "", DateTime.Now.Date);
            var service = new KitSnapshotService(ctx);

            snapshotInput_1.RunDate = new DateTime(2020, 11, 24);
            var payload = await service.GenerateSnapshot(snapshotInput_1);
            Assert.Equal(1, payload.Entity.Sequence);

            snapshotInput_1.RunDate = new DateTime(2020, 11, 25);
            payload = await service.GenerateSnapshot(snapshotInput_1);
            Assert.Equal(2, payload.Entity.Sequence);


            // setup plant 2
            var plantCode_2 = Gen_PlantCode();
            Gen_Plant_Bom_Lot_and_Kits(plantCode_2);

            var vehicle_2 = ctx.Vehicles.Where(t => t.Lot.Plant.Code == plantCode_2).OrderBy(t => t.KitNo).First();
            await AddVehicleTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, vehicle_2.KitNo, "", DateTime.Now.Date);

            var snapshotInput_2 = new KitSnapshotInput {
                PlantCode = plantCode_2,
                RunDate = new DateTime(2020, 12, 1),
                EngineComponentCode = engineCode
            };

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
        public async Task can_get_vehicle_snapshot_dates() {
            // setup
            var service = new KitSnapshotService(ctx);
            var vehicle_1 = ctx.Vehicles.OrderBy(t => t.KitNo).First();
            var vehicle_2 = ctx.Vehicles.OrderBy(t => t.KitNo).Skip(1).First();

            // 1. vehicle snapshot run with no entries
            var snapshotInput = new KitSnapshotInput {
                RunDate = new DateTime(2020, 12, 1),
                PlantCode = ctx.Plants.Select(t => t.Code).First(),
                EngineComponentCode = engineCode
            };
            await service.GenerateSnapshot(snapshotInput);
            var snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            Assert.Null(snapshotPayload);
            var snapshotCount = ctx.VehicleSnapshotRuns.Count();
            Assert.Equal(0, snapshotCount);

            // 2.  custom received
            await AddVehicleTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, vehicle_1.KitNo, "", DateTime.Now.Date);
            await AddVehicleTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, vehicle_2.KitNo, "", DateTime.Now.Date);
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            Assert.Equal(2, snapshotPayload.Entries.Count);
            snapshotCount = ctx.VehicleSnapshotRuns.Count();
            Assert.Equal(1, snapshotCount);

            // 3.  no change
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);

            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);

            var totalSnapshotEntries = await ctx.VehicleSnapshots.CountAsync();
            Assert.Equal(6, totalSnapshotEntries);

            snapshotCount = ctx.VehicleSnapshotRuns.Count();
            Assert.Equal(3, snapshotCount);
        }

        #region test helper methods
        private async Task<List<VehicleSnapshotRunDTO.Entry>> GetVehiclePartnerStatusReport(
            string plantCode,
            string engineComponentCode,
            DateTime date) {

            var snapshotInput = new KitSnapshotInput {
                PlantCode = plantCode,
                EngineComponentCode = engineComponentCode,
                RunDate = date
            };

            // initial
            var service = new KitSnapshotService(ctx);
            var payload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            return payload.Entries.ToList();
        }

        private async Task AddEngineSerialNumberComponentScan(string kitNo, string engineComponentCode, string engineSerial) {
            var engineVehicleComponent = ctx.VehicleComponents
                .First(t => t.Kit.KitNo == kitNo && t.Component.Code == engineComponentCode);
            var scanService = new ComponentSerialService(ctx);
            var createScanPayload = await scanService.CaptureComponentSerial(new ComponentSerialInput {
                VehicleComponentId = engineVehicleComponent.Id,
                Serial1 = engineSerial,
                Serial2 = ""
            });

            var componentSerialResult = createScanPayload.Entity;
            var dcwsService = new DCWSResponseService(ctx);
            await dcwsService.SaveDcwsComponentResponse(new DcwsComponentResponseInput {
                VehicleComponentId = componentSerialResult.ComponentSerialId,
                ResponseCode = "NONE",
                ErrorMessage = ""
            });
        }
        private async Task AddVehicleTimelineEntry(TimeLineEventType eventType, string kitNo, string eventNote, DateTime eventDate) {
            var service = new VehicleService(ctx);
            var payload = await service.CreateKitTimelineEvent(new KitTimelineEventInput {
                KitNo = kitNo,
                EventType = eventType,
                EventNote = eventNote,
                EventDate = eventDate
            });
        }



        #endregion

    }
}