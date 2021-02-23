using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class KitSnapshotServiceTest : TestBase {

        string engineCode = "EN";
        int wholeSateCutOffDays = 7;
        int planBuildLeadTimeDays = 2;

        public KitSnapshotServiceTest() {
            ctx = GetAppDbContext();
            Gen_Baseline_Test_Seed_Data(generateLot: true, componentCodes: new List<string> { "DA", "PA", engineCode });
        }

        [Fact]
        public async Task can_generate_snapshot() {
            // setup
            var baseDate = DateTime.Now.Date;
            var custom_receive_date = baseDate.AddDays(1);
            var custom_receive_date_trx = baseDate.AddDays(2);

            var plantCode = ctx.Plants.Select(t => t.Code).First();
            var kit = ctx.Kits.OrderBy(t => t.KitNo).First();
            var snapshotInput = new KitSnapshotInput {
                PlantCode = plantCode,
                EngineComponentCode = engineCode
            };

            var service = new KitSnapshotService(ctx);
            var payload = await service.GenerateSnapshot(snapshotInput);
            Assert.Equal(0, payload.Entity.SnapshotCount);

            // custom received
            await AddKitTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, kit.KitNo, "", custom_receive_date_trx, custom_receive_date);
            snapshotInput.RunDate = custom_receive_date_trx;
            payload = await service.GenerateSnapshot(snapshotInput);
            var snapshots_count = ctx.KitSnapshots.Count();
            Assert.Equal(1, snapshots_count);
        }

        enum TimelineTestEvent {
            BEFORE,
            CUSTOM_RECEIVED_TRX,
            POST_CUSTOM_RECEIVED_NO_CHANGE,
            PLAN_BUILD_TRX,
            POST_PLAN_BUILD_NO_CHANGE,
            BUILD_COMPLETE_TRX,
            GATE_RELEASE_TRX,
            WHOLE_SALE_TRX,
            FINAL_2_DAYS_TRX,
            FINAL_PLUS_WHOLESALE_CUTOFF
        }

        [Fact]
        public async Task can_create_full_snapshot_timeline() {
            var baseDate = DateTime.Now.Date;
            var dates = new List<(TimelineTestEvent eventType, DateTime date)>() {
                (TimelineTestEvent.BEFORE, baseDate ),
                (TimelineTestEvent.CUSTOM_RECEIVED_TRX, baseDate.AddDays(1) ),
                (TimelineTestEvent.POST_CUSTOM_RECEIVED_NO_CHANGE, baseDate.AddDays(2) ),
                (TimelineTestEvent.PLAN_BUILD_TRX, baseDate.AddDays(3) ),
                (TimelineTestEvent.POST_PLAN_BUILD_NO_CHANGE, baseDate.AddDays(4) ),
                (TimelineTestEvent.BUILD_COMPLETE_TRX, baseDate.AddDays(6) ),
                (TimelineTestEvent.GATE_RELEASE_TRX, baseDate.AddDays(8) ),
                (TimelineTestEvent.WHOLE_SALE_TRX, baseDate.AddDays(10) ),
                (TimelineTestEvent.FINAL_2_DAYS_TRX, baseDate.AddDays(12) ),
                (TimelineTestEvent.FINAL_PLUS_WHOLESALE_CUTOFF, baseDate.AddDays(10 + wholeSateCutOffDays) ),
            };

            // setup
            var service = new KitSnapshotService(ctx);
            var kit = ctx.Kits.OrderBy(t => t.KitNo).First();
            var dealerCode = "DLR_1";
            var plantCode = ctx.Plants.Select(t => t.Code).First();
            DateTime eventDate = DateTime.Now.Date;
            var snapshotInput = new KitSnapshotInput {
                RunDate = new DateTime(2020, 12, 2),
                PlantCode = plantCode,
                EngineComponentCode = engineCode
            };

            // 1.  empty
            snapshotInput.RunDate = dates.Where(t => t.eventType == TimelineTestEvent.BEFORE).First().date;
            await service.GenerateSnapshot(snapshotInput);
            var snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            Assert.Null(snapshotPayload);

            // 2.  custom received
            eventDate = dates.Where(t => t.eventType == TimelineTestEvent.CUSTOM_RECEIVED_TRX).First().date;
            await AddKitTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, kit.KitNo, "", eventDate, eventDate.AddDays(-1));
            snapshotInput.RunDate = eventDate;
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            var kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(TimeLineEventType.CUSTOM_RECEIVED, kitSnapshot.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Added, kitSnapshot.TxType);

            // 2.  custom no change
            eventDate = dates.Where(t => t.eventType == TimelineTestEvent.POST_CUSTOM_RECEIVED_NO_CHANGE).First().date;
            snapshotInput.RunDate = eventDate;
            await service.GenerateSnapshot(snapshotInput);
            var snapshotRecordCount = await ctx.KitSnapshots.CountAsync();
            Assert.Equal(2, snapshotRecordCount);
            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(1, snapshotPayload.Entries.Count);
            Assert.Equal(TimeLineEventType.CUSTOM_RECEIVED, kitSnapshot.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.NoChange, kitSnapshot.TxType);

            // 3.  plan build
            eventDate = dates.Where(t => t.eventType == TimelineTestEvent.PLAN_BUILD_TRX).First().date;
            snapshotInput.RunDate = eventDate;
            await AddKitTimelineEntry(TimeLineEventType.PLAN_BUILD, kit.KitNo, "", eventDate, eventDate);
            await service.GenerateSnapshot(snapshotInput);

            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(TimeLineEventType.PLAN_BUILD, kitSnapshot.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Changed, kitSnapshot.TxType);

            // 4. post plant build no change
            eventDate = dates.Where(t => t.eventType == TimelineTestEvent.POST_PLAN_BUILD_NO_CHANGE).First().date;
            snapshotInput.RunDate = eventDate;
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(TimeLineEventType.PLAN_BUILD, kitSnapshot.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.NoChange, kitSnapshot.TxType);

            // 5. build completed
            eventDate = dates.Where(t => t.eventType == TimelineTestEvent.BUILD_COMPLETE_TRX).First().date;
            snapshotInput.RunDate = eventDate;
            await AddKitTimelineEntry(TimeLineEventType.BULD_COMPLETED, kit.KitNo, "", eventDate, eventDate);
            await service.GenerateSnapshot(snapshotInput);

            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(TimeLineEventType.BULD_COMPLETED, kitSnapshot.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Changed, kitSnapshot.TxType);

            // 5.  gate release
            eventDate = dates.Where(t => t.eventType == TimelineTestEvent.GATE_RELEASE_TRX).First().date;
            snapshotInput.RunDate = eventDate;
            await AddKitTimelineEntry(TimeLineEventType.GATE_RELEASED, kit.KitNo, "", eventDate, eventDate);
            await service.GenerateSnapshot(snapshotInput);

            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(TimeLineEventType.GATE_RELEASED, kitSnapshot.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Changed, kitSnapshot.TxType);


            // 6.  wholesale
            eventDate = dates.Where(t => t.eventType == TimelineTestEvent.WHOLE_SALE_TRX).First().date;
            snapshotInput.RunDate = eventDate;
            await AddKitTimelineEntry(TimeLineEventType.WHOLE_SALE, kit.KitNo, dealerCode, eventDate, eventDate);
            await service.GenerateSnapshot(snapshotInput);

            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(TimeLineEventType.WHOLE_SALE, kitSnapshot.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Final, kitSnapshot.TxType);
            Assert.Equal(PartnerStatus_ChangeStatus.Final, kitSnapshot.TxType);
            Assert.Equal(dealerCode, kitSnapshot.DealerCode);

            // 7.  no change ( should still be final)
            eventDate = dates.Where(t => t.eventType == TimelineTestEvent.FINAL_2_DAYS_TRX).First().date;
            snapshotInput.RunDate = eventDate;
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(TimeLineEventType.WHOLE_SALE, kitSnapshot.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Final, kitSnapshot.TxType);

            // 8.  wholesale
            eventDate = dates.Where(t => t.eventType == TimelineTestEvent.FINAL_PLUS_WHOLESALE_CUTOFF).First().date;
            snapshotInput.RunDate = eventDate;
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            Assert.Null(snapshotPayload);

        }

        [Fact]
        public async Task creates_original_plan_build_date_only_once() {
            // setup
            var service = new KitSnapshotService(ctx);
            var snapshotInput = new KitSnapshotInput {
                PlantCode = ctx.Plants.Select(t => t.Code).First(),
                EngineComponentCode = engineCode
            };
            var kit = ctx.Kits.OrderBy(t => t.KitNo).First();

            var baseDate = DateTime.Now.Date;

            var custom_receive_date = baseDate.AddDays(1);
            var custom_receive_date_trx = baseDate.AddDays(2);

            var plan_build_date_trx = baseDate.AddDays(3);
            var plan_build_date = baseDate.AddDays(4);

            var new_plan_build_date_trx = baseDate.AddDays(5);
            var new_plan_build_date = baseDate.AddDays(6);

            // 1.  custom received
            await AddKitTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, kit.KitNo, "", custom_receive_date_trx, custom_receive_date);
            snapshotInput.RunDate = custom_receive_date_trx;
            var payload = await service.GenerateSnapshot(snapshotInput);
            var snapshotRun = await service.GetSnapshotRunBySequence(snapshotInput.PlantCode, payload.Entity.Sequence.Value);
            var kitSnapshot = snapshotRun.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(1, snapshotRun.Entries.Count);
            Assert.Equal(TimeLineEventType.CUSTOM_RECEIVED, kitSnapshot.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Added, kitSnapshot.TxType);

            // 1.  plan build 
            await AddKitTimelineEntry(TimeLineEventType.PLAN_BUILD, kit.KitNo, "", plan_build_date_trx, plan_build_date);
            snapshotInput.RunDate = plan_build_date_trx;
            payload = await service.GenerateSnapshot(snapshotInput);
            snapshotRun = await service.GetSnapshotRunBySequence(snapshotInput.PlantCode, payload.Entity.Sequence.Value);
            kitSnapshot = snapshotRun.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(1, snapshotRun.Entries.Count);
            Assert.Equal(TimeLineEventType.PLAN_BUILD, kitSnapshot.CurrentTimelineEvent);
            Assert.Equal(plan_build_date, kitSnapshot.PlanBuild);
            Assert.Equal(plan_build_date, kitSnapshot.OriginalPlanBuild);
            Assert.Equal(PartnerStatus_ChangeStatus.Changed, kitSnapshot.TxType);


            // 1. edit plan build date does not change original
            await AddKitTimelineEntry(TimeLineEventType.PLAN_BUILD, kit.KitNo, "", new_plan_build_date_trx, new_plan_build_date);
            snapshotInput.RunDate = new_plan_build_date_trx;
            payload = await service.GenerateSnapshot(snapshotInput);
            snapshotRun = await service.GetSnapshotRunBySequence(snapshotInput.PlantCode, payload.Entity.Sequence.Value);
            kitSnapshot = snapshotRun.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(1, snapshotRun.Entries.Count);
            Assert.Equal(TimeLineEventType.PLAN_BUILD, kitSnapshot.CurrentTimelineEvent);
            Assert.Equal(new_plan_build_date, kitSnapshot.PlanBuild);
            Assert.Equal(plan_build_date, kitSnapshot.OriginalPlanBuild);
            Assert.Equal(PartnerStatus_ChangeStatus.NoChange, kitSnapshot.TxType);
        }

        [Fact]
        public async Task kit_can_have_multiple_timeline_events_before_first_snapshot() {
            // setup
            var eventDate = new DateTime(2020, 12, 1);
            var snapshotInput = new KitSnapshotInput {
                RunDate = new DateTime(2020, 12, 1),
                PlantCode = ctx.Plants.Select(t => t.Code).First(),
                EngineComponentCode = engineCode
            };

            var baseDate = DateTime.Now.Date;

            var custom_receive_date = baseDate.AddDays(1);
            var custom_receive_date_trx = baseDate.AddDays(2);

            var plan_build_date_trx = baseDate.AddDays(3);
            var plan_build_date = baseDate.AddDays(4);

            var kit = ctx.Kits.OrderBy(t => t.KitNo).First();
            await AddKitTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, kit.KitNo, "", custom_receive_date_trx, custom_receive_date);
            await AddKitTimelineEntry(TimeLineEventType.PLAN_BUILD, kit.KitNo, "", plan_build_date_trx, plan_build_date);

            var service = new KitSnapshotService(ctx);
            await service.GenerateSnapshot(snapshotInput);

            var snapshots_count = ctx.KitSnapshots.Count();
            Assert.Equal(1, snapshots_count);

            var snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            var kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(TimeLineEventType.PLAN_BUILD, kitSnapshot.CurrentTimelineEvent);
            Assert.Equal(PartnerStatus_ChangeStatus.Added, kitSnapshot.TxType);
        }

        [Fact]
        public async Task cannot_generate_snapshot_with_same_run_date() {
            // setup
            var kit = ctx.Kits.OrderBy(t => t.KitNo).First();

            var baseDate = DateTime.Now.Date;

            var custom_receive_date = baseDate.AddDays(1);
            var custom_receive_date_trx = baseDate.AddDays(2);

            await AddKitTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, kit.KitNo, "", custom_receive_date_trx, custom_receive_date);
            var service = new KitSnapshotService(ctx);

            var snapshotInput = new KitSnapshotInput {
                RunDate = custom_receive_date_trx,
                PlantCode = ctx.Plants.Select(t => t.Code).First(),
                EngineComponentCode = engineCode
            };
            var payload = await service.GenerateSnapshot(snapshotInput);
            var snapshots_count = ctx.KitSnapshots.Count();
            Assert.Equal(1, snapshots_count);

            // test with same runDate
            payload = await service.GenerateSnapshot(snapshotInput);

            var errorCount = payload.Errors.Count;
            Assert.Equal(1, errorCount);

        }

        [Fact]
        public async Task generate_snapshot_with_same_plant_code_increments_sequence() {
            // setup 
            var baseDate = DateTime.Now.Date;

            var custom_receive_date = baseDate.AddDays(1);
            var custom_receive_date_trx = baseDate.AddDays(2);

            // plant 1
            var plantCode_1 = ctx.Plants.Select(t => t.Code).First();

            var snapshotInput_1 = new KitSnapshotInput {
                PlantCode = plantCode_1,
                EngineComponentCode = engineCode
            };

            var kit_1 = ctx.Kits.Where(t => t.Lot.Plant.Code == plantCode_1).OrderBy(t => t.KitNo).First();
            await AddKitTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, kit_1.KitNo, "", custom_receive_date_trx, custom_receive_date);
            var service = new KitSnapshotService(ctx);

            snapshotInput_1.RunDate = new DateTime(2020, 11, 24);
            var payload = await service.GenerateSnapshot(snapshotInput_1);
            Assert.Equal(1, payload.Entity.Sequence);

            snapshotInput_1.RunDate = new DateTime(2020, 11, 25);
            payload = await service.GenerateSnapshot(snapshotInput_1);
            Assert.Equal(2, payload.Entity.Sequence);

            // setup plant 2
            var plantCode_2 = Gen_PlantCode();         
            
            Gen_Bom_Lot_and_Kits(plantCode_2);

            var kit_2 = ctx.Kits.Where(t => t.Lot.Plant.Code == plantCode_2).OrderBy(t => t.KitNo).First();
            await AddKitTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, kit_2.KitNo, "", custom_receive_date_trx, custom_receive_date);

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
            var kit_snapshot_runs = await ctx.KitSnapshotRuns.CountAsync();
            Assert.Equal(4, kit_snapshot_runs);
        }

        [Fact]
        public async Task can_get_vehicle_snapshot_dates() {
            // setup
            var plantCode = ctx.Plants.Select(t => t.Code).First();
            var baseDate = DateTime.Now.Date;

            var custom_receive_date = baseDate.AddDays(1);
            var custom_receive_date_trx = baseDate.AddDays(2);

            var service = new KitSnapshotService(ctx);
            var kit_1 = ctx.Kits.OrderBy(t => t.KitNo).First();
            var kit_2 = ctx.Kits.OrderBy(t => t.KitNo).Skip(1).First();

            // 1. kit snapshot run with no entries
            var snapshotInput = new KitSnapshotInput {
                PlantCode = plantCode,
                EngineComponentCode = engineCode
            };
            snapshotInput.RunDate = baseDate;
            await service.GenerateSnapshot(snapshotInput);
            var snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            Assert.Null(snapshotPayload);
            var snapshotCount = ctx.KitSnapshotRuns.Count();
            Assert.Equal(0, snapshotCount);

            // 2.  custom received
            await AddKitTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, kit_1.KitNo, "", custom_receive_date_trx, custom_receive_date);
            await AddKitTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, kit_2.KitNo, "", custom_receive_date_trx, custom_receive_date);
            snapshotInput.RunDate = custom_receive_date_trx;
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate);
            Assert.Equal(2, snapshotPayload.Entries.Count);
            snapshotCount = ctx.KitSnapshotRuns.Count();
            Assert.Equal(1, snapshotCount);

            // 3.  no change
            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);

            snapshotInput.RunDate = snapshotInput.RunDate.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);

            var totalSnapshotEntries = await ctx.KitSnapshots.CountAsync();
            Assert.Equal(6, totalSnapshotEntries);

            snapshotCount = ctx.KitSnapshotRuns.Count();
            Assert.Equal(3, snapshotCount);
        }

        #region test helper methods
        private async Task<List<KitSnapshotRunDTO.Entry>> GetVehiclePartnerStatusReport(
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
            var engineVehicleComponent = ctx.KitComponents
                .First(t => t.Kit.KitNo == kitNo && t.Component.Code == engineComponentCode);
            var scanService = new ComponentSerialService(ctx);
            var createScanPayload = await scanService.CaptureComponentSerial(new ComponentSerialInput {
                KitComponentId = engineVehicleComponent.Id,
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
        private async Task AddKitTimelineEntry(
            TimeLineEventType eventType,
            string kitNo,
            string eventNote,
            DateTime trxDate,
            DateTime eventDate
        ) {
            var service = new KitService(ctx, trxDate, planBuildLeadTimeDays);
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