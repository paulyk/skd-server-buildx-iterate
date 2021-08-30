using System;
using System.Collections.Generic;
using SKD.Model;
using SKD.Service;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using SKD.Dcws;
using SKD.Common;

namespace SKD.Test {
    public class KitSnapshotServiceTest : TestBase {

        readonly string engineCode = "EN";
        readonly int wholeSateCutOffDays = 7;
        readonly int planBuildLeadTimeDays = 2;

        public KitSnapshotServiceTest() {
            context = GetAppDbContext();
            Gen_Baseline_Test_Seed_Data(generateLot: true, componentCodes: new List<string> { "DA", "PA", engineCode });
        }

        [Fact]
        public async Task Can_generate_snapshot() {
            // setup
            var baseDate = DateTime.Now.Date;
            var custom_receive_date = baseDate.AddDays(1);
            var custom_receive_date_trx = baseDate.AddDays(2);

            var plantCode = context.Plants.Select(t => t.Code).First();
            var kit = context.Kits.OrderBy(t => t.KitNo).First();
            var snapshotInput = new KitSnapshotInput {
                PlantCode = plantCode,
                EngineComponentCode = engineCode
            };

            var service = new KitSnapshotService(context);
            var payload = await service.GenerateSnapshot(snapshotInput);
            Assert.Equal(0, payload.Payload.SnapshotCount);

            // custom received
            await AddKitTimelineEntry(TimeLineEventCode.CUSTOM_RECEIVED, kit.KitNo, "", custom_receive_date_trx, custom_receive_date);
            snapshotInput.RunDate = custom_receive_date_trx;
            payload = await service.GenerateSnapshot(snapshotInput);
            var snapshots_count = context.KitSnapshots.Count();
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
        public async Task Can_create_full_snapshot_timeline() {
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
            var service = new KitSnapshotService(context);
            var kit = context.Kits.OrderBy(t => t.KitNo).First();
            var dealerCode = "DLR_1";
            var plantCode = context.Plants.Select(t => t.Code).First();
            DateTime eventDate = DateTime.Now.Date;
            var snapshotInput = new KitSnapshotInput {
                RunDate = new DateTime(2020, 12, 2),
                PlantCode = plantCode,
                EngineComponentCode = engineCode,
                RejectIfNoChanges = false
            };

            // 1.  empty
            snapshotInput.RunDate = dates.Where(t => t.eventType == TimelineTestEvent.BEFORE).First().date;
            await service.GenerateSnapshot(snapshotInput);
            var snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
            Assert.Null(snapshotPayload);

            // 2.  custom received
            eventDate = dates.Where(t => t.eventType == TimelineTestEvent.CUSTOM_RECEIVED_TRX).First().date;
            await AddKitTimelineEntry(TimeLineEventCode.CUSTOM_RECEIVED, kit.KitNo, "", eventDate, eventDate.AddDays(-1));
            snapshotInput.RunDate = eventDate;
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
            var kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(TimeLineEventCode.CUSTOM_RECEIVED, kitSnapshot.CurrentTimeLineCode);
            Assert.Equal(PartnerStatus_ChangeStatus.Added, kitSnapshot.TxType);

            // 2.  custom no change
            eventDate = dates.Where(t => t.eventType == TimelineTestEvent.POST_CUSTOM_RECEIVED_NO_CHANGE).First().date;
            snapshotInput.RunDate = eventDate;
            await service.GenerateSnapshot(snapshotInput);
            var snapshotRecordCount = await context.KitSnapshots.CountAsync();
            Assert.Equal(2, snapshotRecordCount);
            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
            kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(1, snapshotPayload.Entries.Count);
            Assert.Equal(TimeLineEventCode.CUSTOM_RECEIVED, kitSnapshot.CurrentTimeLineCode);
            Assert.Equal(PartnerStatus_ChangeStatus.NoChange, kitSnapshot.TxType);

            // 3.  plan build
            eventDate = dates.Where(t => t.eventType == TimelineTestEvent.PLAN_BUILD_TRX).First().date;
            snapshotInput.RunDate = eventDate;
            await AddKitTimelineEntry(TimeLineEventCode.PLAN_BUILD, kit.KitNo, "", eventDate, eventDate);
            await service.GenerateSnapshot(snapshotInput);

            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
            kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(TimeLineEventCode.PLAN_BUILD, kitSnapshot.CurrentTimeLineCode);
            Assert.Equal(PartnerStatus_ChangeStatus.Changed, kitSnapshot.TxType);

            // 4. post plant build no change
            eventDate = dates.Where(t => t.eventType == TimelineTestEvent.POST_PLAN_BUILD_NO_CHANGE).First().date;
            snapshotInput.RunDate = eventDate;
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
            kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(TimeLineEventCode.PLAN_BUILD, kitSnapshot.CurrentTimeLineCode);
            Assert.Equal(PartnerStatus_ChangeStatus.NoChange, kitSnapshot.TxType);

            // 5. build completed
            eventDate = dates.Where(t => t.eventType == TimelineTestEvent.BUILD_COMPLETE_TRX).First().date;
            snapshotInput.RunDate = eventDate;
            await AddKitTimelineEntry(TimeLineEventCode.BUILD_COMPLETED, kit.KitNo, "", eventDate, eventDate);
            await service.GenerateSnapshot(snapshotInput);

            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
            kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(TimeLineEventCode.BUILD_COMPLETED, kitSnapshot.CurrentTimeLineCode);
            Assert.Equal(PartnerStatus_ChangeStatus.Changed, kitSnapshot.TxType);

            // 5.  gate release
            eventDate = dates.Where(t => t.eventType == TimelineTestEvent.GATE_RELEASE_TRX).First().date;
            snapshotInput.RunDate = eventDate;
            await AddKitTimelineEntry(TimeLineEventCode.GATE_RELEASED, kit.KitNo, "", eventDate, eventDate);
            await service.GenerateSnapshot(snapshotInput);

            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
            kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(TimeLineEventCode.GATE_RELEASED, kitSnapshot.CurrentTimeLineCode);
            Assert.Equal(PartnerStatus_ChangeStatus.Changed, kitSnapshot.TxType);


            // 6.  wholesale
            eventDate = dates.Where(t => t.eventType == TimelineTestEvent.WHOLE_SALE_TRX).First().date;
            snapshotInput.RunDate = eventDate;
            await AddKitTimelineEntry(TimeLineEventCode.WHOLE_SALE, kit.KitNo, dealerCode, eventDate, eventDate);
            await service.GenerateSnapshot(snapshotInput);

            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
            kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(TimeLineEventCode.WHOLE_SALE, kitSnapshot.CurrentTimeLineCode);
            Assert.Equal(PartnerStatus_ChangeStatus.Final, kitSnapshot.TxType);
            Assert.Equal(PartnerStatus_ChangeStatus.Final, kitSnapshot.TxType);
            Assert.Equal(dealerCode, kitSnapshot.DealerCode);

            // 7.  no change ( should still be final)
            eventDate = dates.Where(t => t.eventType == TimelineTestEvent.FINAL_2_DAYS_TRX).First().date;
            snapshotInput.RunDate = eventDate;
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
            kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(TimeLineEventCode.WHOLE_SALE, kitSnapshot.CurrentTimeLineCode);
            Assert.Equal(PartnerStatus_ChangeStatus.Final, kitSnapshot.TxType);

            // 8.  wholesale
            eventDate = dates.Where(t => t.eventType == TimelineTestEvent.FINAL_PLUS_WHOLESALE_CUTOFF).First().date;
            snapshotInput.RunDate = eventDate;
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
            Assert.Null(snapshotPayload);

        }

        [Fact]
        public async Task Creates_original_plan_build_date_only_once() {
            // setup
            var service = new KitSnapshotService(context);
            var snapshotInput = new KitSnapshotInput {
                PlantCode = context.Plants.Select(t => t.Code).First(),
                EngineComponentCode = engineCode
            };
            var kit = context.Kits.OrderBy(t => t.KitNo).First();

            var baseDate = DateTime.Now.Date;

            var custom_receive_date = baseDate.AddDays(-1);
            var custom_receive_date_trx = baseDate;

            var plan_build_date_trx = baseDate.AddDays(1);
            var plan_build_date = custom_receive_date.AddDays(planBuildLeadTimeDays);

            var new_plan_build_date_trx = baseDate.AddDays(2);
            var new_plan_build_date = custom_receive_date.AddDays(planBuildLeadTimeDays + 2);

            // 1.  custom received
            await AddKitTimelineEntry(TimeLineEventCode.CUSTOM_RECEIVED, kit.KitNo, "note", custom_receive_date_trx, custom_receive_date);
            snapshotInput.RunDate = custom_receive_date_trx;
            var payload = await service.GenerateSnapshot(snapshotInput);
            var snapshotRun = await service.GetSnapshotRunBySequence(snapshotInput.PlantCode, payload.Payload.Sequence);
            var kitSnapshot = snapshotRun.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(1, snapshotRun.Entries.Count);
            Assert.Equal(TimeLineEventCode.CUSTOM_RECEIVED, kitSnapshot.CurrentTimeLineCode);
            Assert.Equal(PartnerStatus_ChangeStatus.Added, kitSnapshot.TxType);

            // 1.  plan build 
            await AddKitTimelineEntry(TimeLineEventCode.PLAN_BUILD, kit.KitNo, "note", plan_build_date_trx, plan_build_date);
            snapshotInput.RunDate = plan_build_date_trx;
            payload = await service.GenerateSnapshot(snapshotInput);
            snapshotRun = await service.GetSnapshotRunBySequence(snapshotInput.PlantCode, payload.Payload.Sequence);
            kitSnapshot = snapshotRun.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(1, snapshotRun.Entries.Count);
            Assert.Equal(TimeLineEventCode.PLAN_BUILD, kitSnapshot.CurrentTimeLineCode);
            Assert.Equal(plan_build_date, kitSnapshot.PlanBuild);
            Assert.Equal(plan_build_date, kitSnapshot.OriginalPlanBuild);
            Assert.Equal(PartnerStatus_ChangeStatus.Changed, kitSnapshot.TxType);


            // 1. edit plan build date does not change original
            var payload_2 = await AddKitTimelineEntry(TimeLineEventCode.PLAN_BUILD, kit.KitNo, "note", new_plan_build_date_trx, new_plan_build_date);
            var expectedErrorMessage = "cannot change date after snapshot taken";
            var actualErrorMessage = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
            Assert.Equal(expectedErrorMessage, actualErrorMessage);

            // disable the following until we allow users to modify timeline events after snapwhot taken
            /*
            snapshotInput.RunDate = new_plan_build_date_trx;
            payload = await service.GenerateSnapshot(snapshotInput);
            snapshotRun = await service.GetSnapshotRunBySequence(snapshotInput.PlantCode, payload.Entity.Sequence.Value);
            kitSnapshot = snapshotRun.Entries.First(t => t.KitNo == kit.KitNo);
            Assert.Equal(1, snapshotRun.Entries.Count);
            Assert.Equal(TimeLineEventType.PLAN_BUILD, kitSnapshot.CurrentTimelineEvent);
            Assert.Equal(new_plan_build_date, kitSnapshot.PlanBuild);
            Assert.Equal(plan_build_date, kitSnapshot.OriginalPlanBuild);
            Assert.Equal(PartnerStatus_ChangeStatus.NoChange, kitSnapshot.TxType);
            */
        }

        ///<remarks>
        /// Allow multiple timeline events to be entered between snapshots.
        /// THe next snapshot generated selects the timline event status based on KitTimelineEventType sequence
        ///</remarks>
        [Fact]
        public async Task Kit_can_have_multiple_timeline_events_before_first_snapshot() {
            // setup
            var plantCode = context.Plants.Select(t => t.Code).First();
            var baseDate = DateTime.Now.Date;
            var snapshotInput = new KitSnapshotInput {
                RunDate = baseDate,
                PlantCode = plantCode,
                EngineComponentCode = engineCode,
                RejectIfNoChanges = false
            };
            var base_date = baseDate.AddDays(1);

            var first_trx_date = baseDate.AddDays(4);
            var plan_build_date = baseDate.AddDays(10);
            var build_completed_date = baseDate.AddDays(12);

            var kit = context.Kits.OrderBy(t => t.KitNo).First();
            await AddKitTimelineEntry(TimeLineEventCode.CUSTOM_RECEIVED, kit.KitNo, "", first_trx_date, base_date);
            await AddKitTimelineEntry(TimeLineEventCode.PLAN_BUILD, kit.KitNo, "", first_trx_date, plan_build_date);
            await AddKitTimelineEntry(TimeLineEventCode.BUILD_COMPLETED, kit.KitNo, "", first_trx_date, build_completed_date);

            var snapShotService = new KitSnapshotService(context);
            var partnerStatusBuilder = new PartnerStatusBuilder(context);
            var detailLineParser = new FlatFileLine<PartnerStatusLayout.Detail>();

            MutationPayload<SnapshotDTO> snapShotPayload = null;
            KitSnapshotRunDTO snapshotRun = null;
            int snapshotCount = 0;
            KitSnapshotRunDTO.Entry kitSnapshot = null;

            // day 1 only custom receive set
            await AssertDay_1();
            await AssertDay_2();
            await AssertDay_3();
            await AssertDay_4();

            async Task AssertDay_1() {
                snapShotPayload = await snapShotService.GenerateSnapshot(snapshotInput);

                snapshotRun = await snapShotService.GetSnapshotRunBySequence(snapshotInput.PlantCode, snapShotPayload.Payload.Sequence);
                snapshotCount = await context.KitSnapshots.CountAsync();
                kitSnapshot = snapshotRun.Entries.First(t => t.KitNo == kit.KitNo);

                Assert.Equal(1, snapshotCount);
                Assert.Equal(TimeLineEventCode.CUSTOM_RECEIVED, kitSnapshot.CurrentTimeLineCode);
                Assert.Equal(PartnerStatus_ChangeStatus.Added, kitSnapshot.TxType);
                Assert.Null(kitSnapshot.PlanBuild);
                Assert.Null(kitSnapshot.OriginalPlanBuild);

                var partnerStatusDTO = await partnerStatusBuilder.GeneratePartnerStatusFilePaylaod(snapshotRun.PlantCode, snapshotRun.Sequence);
                var detailLine = partnerStatusDTO.PayloadText.Split('\n')[1];
                var customReceiveDate = detailLineParser.GetFieldValue(detailLine, t => t.PST_FPRE_STATUS_DATE);

                var originalPlanBuild = detailLineParser.GetFieldValue(detailLine, t => t.PST_BUILD_DATE);
                var planBuildDate = detailLineParser.GetFieldValue(detailLine, t => t.PST_FPBP_STATUS_DATE);
                var buildCompleteDate = detailLineParser.GetFieldValue(detailLine, t => t.PST_FPBC_STATUS_DATE);
                Assert.True(!String.IsNullOrWhiteSpace(customReceiveDate));
                Assert.True(String.IsNullOrWhiteSpace(planBuildDate));
                Assert.True(String.IsNullOrWhiteSpace(buildCompleteDate));
                Assert.True(String.IsNullOrWhiteSpace(originalPlanBuild));
            }

            async Task AssertDay_2() {
                snapshotInput.RunDate = snapshotInput.RunDate.Value.AddDays(1);
                snapShotPayload = await snapShotService.GenerateSnapshot(snapshotInput);
                snapshotRun = await snapShotService.GetSnapshotRunBySequence(snapshotInput.PlantCode, snapShotPayload.Payload.Sequence);
                kitSnapshot = snapshotRun.Entries.First(t => t.KitNo == kit.KitNo);
                snapshotCount = await context.KitSnapshots.CountAsync();
                Assert.Equal(2, snapshotCount);
                Assert.Equal(TimeLineEventCode.PLAN_BUILD, kitSnapshot.CurrentTimeLineCode);
                Assert.Equal(PartnerStatus_ChangeStatus.Changed, kitSnapshot.TxType);

                var partnerStatusDTO = await partnerStatusBuilder.GeneratePartnerStatusFilePaylaod(snapshotRun.PlantCode, snapshotRun.Sequence);
                var detailLine = partnerStatusDTO.PayloadText.Split('\n')[1];
                var customReceiveDate = detailLineParser.GetFieldValue(detailLine, t => t.PST_FPRE_STATUS_DATE);
                var planBuildDate = detailLineParser.GetFieldValue(detailLine, t => t.PST_FPBP_STATUS_DATE);
                var buildCompleteDate = detailLineParser.GetFieldValue(detailLine, t => t.PST_FPBC_STATUS_DATE);
                var originalPlanBuild = detailLineParser.GetFieldValue(detailLine, t => t.PST_BUILD_DATE);
                Assert.True(!String.IsNullOrWhiteSpace(customReceiveDate));
                Assert.True(!String.IsNullOrWhiteSpace(planBuildDate));
                Assert.True(String.IsNullOrWhiteSpace(buildCompleteDate));
                Assert.True(!String.IsNullOrWhiteSpace(originalPlanBuild));
            }

            async Task AssertDay_3() {
                snapshotInput.RunDate = snapshotInput.RunDate.Value.AddDays(1);
                snapShotPayload = await snapShotService.GenerateSnapshot(snapshotInput);
                snapshotRun = await snapShotService.GetSnapshotRunBySequence(snapshotInput.PlantCode, snapShotPayload.Payload.Sequence);
                kitSnapshot = snapshotRun.Entries.First(t => t.KitNo == kit.KitNo);
                snapshotCount = await context.KitSnapshots.CountAsync();
                Assert.Equal(3, snapshotCount);
                Assert.Equal(TimeLineEventCode.BUILD_COMPLETED, kitSnapshot.CurrentTimeLineCode);
                Assert.Equal(PartnerStatus_ChangeStatus.Changed, kitSnapshot.TxType);

                var partnerStatusDTO = await partnerStatusBuilder.GeneratePartnerStatusFilePaylaod(snapshotRun.PlantCode, snapshotRun.Sequence);
                var detailLine = partnerStatusDTO.PayloadText.Split('\n')[1];
                var customReceiveDate = detailLineParser.GetFieldValue(detailLine, t => t.PST_FPRE_STATUS_DATE);
                var planBuildDate = detailLineParser.GetFieldValue(detailLine, t => t.PST_FPBP_STATUS_DATE);
                var buildCompleteDate = detailLineParser.GetFieldValue(detailLine, t => t.PST_FPBC_STATUS_DATE);
                var originalPlanBuild = detailLineParser.GetFieldValue(detailLine, t => t.PST_BUILD_DATE);

                Assert.True(!String.IsNullOrWhiteSpace(customReceiveDate));
                Assert.True(!String.IsNullOrWhiteSpace(planBuildDate));
                Assert.True(!String.IsNullOrWhiteSpace(buildCompleteDate));
                Assert.True(!String.IsNullOrWhiteSpace(originalPlanBuild));

            }

            async Task AssertDay_4() {
                snapshotInput.RunDate = snapshotInput.RunDate.Value.AddDays(1);
                snapShotPayload = await snapShotService.GenerateSnapshot(snapshotInput);
                snapshotRun = await snapShotService.GetSnapshotRunBySequence(snapshotInput.PlantCode, snapShotPayload.Payload.Sequence);
                kitSnapshot = snapshotRun.Entries.First(t => t.KitNo == kit.KitNo);
                snapshotCount = await context.KitSnapshots.CountAsync();
                Assert.Equal(4, snapshotCount);
                Assert.Equal(TimeLineEventCode.BUILD_COMPLETED, kitSnapshot.CurrentTimeLineCode);
                Assert.Equal(PartnerStatus_ChangeStatus.NoChange, kitSnapshot.TxType);

                var partnerStatusDTO = await partnerStatusBuilder.GeneratePartnerStatusFilePaylaod(snapshotRun.PlantCode, snapshotRun.Sequence);
                var detailLine = partnerStatusDTO.PayloadText.Split('\n')[1];
                var customReceiveDate = detailLineParser.GetFieldValue(detailLine, t => t.PST_FPRE_STATUS_DATE);
                var planBuildDate = detailLineParser.GetFieldValue(detailLine, t => t.PST_FPBP_STATUS_DATE);
                var buildCompleteDate = detailLineParser.GetFieldValue(detailLine, t => t.PST_FPBC_STATUS_DATE);
                Assert.True(!String.IsNullOrWhiteSpace(customReceiveDate));
                Assert.True(!String.IsNullOrWhiteSpace(planBuildDate));
                Assert.True(!String.IsNullOrWhiteSpace(buildCompleteDate));

            }

        }


        [Fact]
        public async Task Cannot_generate_snapshot_with_same_run_date() {
            // setup
            var kit = context.Kits.OrderBy(t => t.KitNo).First();

            var baseDate = DateTime.Now.Date;

            var custom_receive_date = baseDate.AddDays(1);
            var custom_receive_date_trx = baseDate.AddDays(2);

            await AddKitTimelineEntry(TimeLineEventCode.CUSTOM_RECEIVED, kit.KitNo, "", custom_receive_date_trx, custom_receive_date);
            var service = new KitSnapshotService(context);

            var snapshotInput = new KitSnapshotInput {
                RunDate = custom_receive_date_trx,
                PlantCode = context.Plants.Select(t => t.Code).First(),
                EngineComponentCode = engineCode
            };
            var payload = await service.GenerateSnapshot(snapshotInput);
            var snapshots_count = context.KitSnapshots.Count();
            Assert.Equal(1, snapshots_count);

            // test with same runDate
            payload = await service.GenerateSnapshot(snapshotInput);

            var errorCount = payload.Errors.Count;
            Assert.Equal(1, errorCount);

        }

        [Fact]
        public async Task Generate_snapshot_with_same_plant_code_increments_sequence() {
            // setup 
            var baseDate = DateTime.Now.Date;

            var custom_receive_date = baseDate.AddDays(-1);
            var custom_receive_date_trx = baseDate;
            var run_date_1 = baseDate;
            var run_date_2 = baseDate.AddDays(1);
            var service = new KitSnapshotService(context);

            // plant 1P
            var plantCode = context.Plants.Select(t => t.Code).First();

            var snapshotInput = new KitSnapshotInput {
                PlantCode = plantCode,
                EngineComponentCode = engineCode,
                RejectIfNoChanges = false
            };

            var kit = context.Kits.Where(t => t.Lot.Plant.Code == plantCode).OrderBy(t => t.KitNo).First();
            await AddKitTimelineEntry(TimeLineEventCode.CUSTOM_RECEIVED, kit.KitNo, "", custom_receive_date_trx, custom_receive_date);

            snapshotInput.RunDate = run_date_1;
            var payload = await service.GenerateSnapshot(snapshotInput);
            Assert.Equal(1, payload.Payload.Sequence);

            snapshotInput.RunDate = run_date_2;
            payload = await service.GenerateSnapshot(snapshotInput);
            Assert.Equal(2, payload.Payload.Sequence);

            // setup plant 2
            plantCode = Gen_PlantCode();
            Gen_Bom_Lot_and_Kits(plantCode);

            // plant 1

            snapshotInput = new KitSnapshotInput {
                PlantCode = plantCode,
                EngineComponentCode = engineCode,
                RejectIfNoChanges = false
            };

            kit = context.Kits.Where(t => t.Lot.Plant.Code == plantCode).OrderBy(t => t.KitNo).First();
            await AddKitTimelineEntry(TimeLineEventCode.CUSTOM_RECEIVED, kit.KitNo, "", custom_receive_date_trx, custom_receive_date);

            snapshotInput.RunDate = run_date_1;
            payload = await service.GenerateSnapshot(snapshotInput);
            Assert.Equal(1, payload.Payload.Sequence);

            snapshotInput.RunDate = run_date_2;
            payload = await service.GenerateSnapshot(snapshotInput);
            Assert.Equal(2, payload.Payload.Sequence);


            // total snapshot run entries
            var kit_snapshot_runs = await context.KitSnapshotRuns.CountAsync();
            Assert.Equal(4, kit_snapshot_runs);
        }

        [Fact]
        public async Task Cannot_add_timline_event_if_snapshot_already_generated_with_that_event() {

            // setup
            var service = new KitSnapshotService(context);
            var kit = context.Kits.OrderBy(t => t.KitNo).First();
            var plantCode = context.Plants.Select(t => t.Code).First();
            var customReiveDate = DateTime.Now.Date;
            var buildDays = 7;

            var eventList = new List<(TimeLineEventCode eventType, DateTime trxDate, DateTime eventDate)>() {
                (TimeLineEventCode.CUSTOM_RECEIVED, customReiveDate.AddDays(1), customReiveDate),
                (TimeLineEventCode.PLAN_BUILD,      customReiveDate.AddDays(2), customReiveDate.AddDays(planBuildLeadTimeDays)),
                (TimeLineEventCode.BUILD_COMPLETED,  customReiveDate.AddDays(planBuildLeadTimeDays + buildDays), customReiveDate.AddDays(planBuildLeadTimeDays + buildDays)),
            };

            var expecedErrorMessage = "cannot change date after snapshot taken";
            foreach (var (eventType, trxDate, eventDate) in eventList) {

                var payload = await AddKitTimelineEntry(eventType, kit.KitNo, "", trxDate, eventDate);
                var actualErrorCount = payload.Errors.Count;
                Assert.Equal(0, actualErrorCount);
                await service.GenerateSnapshot(new KitSnapshotInput {
                    PlantCode = plantCode,
                    RunDate = trxDate,
                    EngineComponentCode = engineCode
                });
                // edit date
                var payload_2 = await AddKitTimelineEntry(eventType, kit.KitNo, "", trxDate.AddDays(1), eventDate.AddDays(1));
                var actualErrorMessage = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
                Assert.Equal(expecedErrorMessage, actualErrorMessage);
            }
        }

        [Fact]
        public async Task Can_get_vehicle_snapshot_dates() {
            // setup
            var plantCode = context.Plants.Select(t => t.Code).First();
            var baseDate = DateTime.Now.Date;

            var custom_receive_date = baseDate.AddDays(1);
            var custom_receive_date_trx = baseDate.AddDays(2);

            var service = new KitSnapshotService(context);
            var kit_1 = context.Kits.OrderBy(t => t.KitNo).First();
            var kit_2 = context.Kits.OrderBy(t => t.KitNo).Skip(1).First();

            // 1. kit snapshot run with no entries
            var snapshotInput = new KitSnapshotInput {
                PlantCode = plantCode,
                EngineComponentCode = engineCode,
                RejectIfNoChanges = false
            };
            snapshotInput.RunDate = baseDate;
            await service.GenerateSnapshot(snapshotInput);
            var snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
            Assert.Null(snapshotPayload);
            var snapshotCount = context.KitSnapshotRuns.Count();
            Assert.Equal(0, snapshotCount);

            // 2.  custom received
            await AddKitTimelineEntry(TimeLineEventCode.CUSTOM_RECEIVED, kit_1.KitNo, "", custom_receive_date_trx, custom_receive_date);
            await AddKitTimelineEntry(TimeLineEventCode.CUSTOM_RECEIVED, kit_2.KitNo, "", custom_receive_date_trx, custom_receive_date);
            snapshotInput.RunDate = custom_receive_date_trx;
            await service.GenerateSnapshot(snapshotInput);
            snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
            Assert.Equal(2, snapshotPayload.Entries.Count);
            snapshotCount = context.KitSnapshotRuns.Count();
            Assert.Equal(1, snapshotCount);

            // 3.  no change will be rejected
            snapshotInput.RunDate = snapshotInput.RunDate.Value.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);

            snapshotInput.RunDate = snapshotInput.RunDate.Value.AddDays(1);
            await service.GenerateSnapshot(snapshotInput);

            var totalSnapshotEntries = await context.KitSnapshots.CountAsync();
            Assert.Equal(6, totalSnapshotEntries);

            snapshotCount = context.KitSnapshotRuns.Count();
            Assert.Equal(3, snapshotCount);
        }

        [Fact]
        public async Task Reject_kit_snapshot_generation_if_no_changes() {
            // setup
            var plantCode = context.Plants.Select(t => t.Code).First();
            var baseDate = DateTime.Now.Date;

            var custom_receive_date = baseDate.AddDays(1);
            var custom_receive_date_trx = baseDate.AddDays(2);
            var plan_build_date = baseDate.AddDays(8);
            var plan_build_date_trx = baseDate.AddDays(3);

            var service = new KitSnapshotService(context);
            var kit_1 = context.Kits.OrderBy(t => t.KitNo).First();
            var kit_2 = context.Kits.OrderBy(t => t.KitNo).Skip(1).First();

            var snapshotInput = new KitSnapshotInput {
                PlantCode = plantCode,
                EngineComponentCode = engineCode,
                RejectIfNoChanges = true,
                RunDate = baseDate.AddDays(3)
            };

            await AddKitTimelineEntry(TimeLineEventCode.CUSTOM_RECEIVED, kit_1.KitNo, "", custom_receive_date_trx, custom_receive_date);
            await AddKitTimelineEntry(TimeLineEventCode.CUSTOM_RECEIVED, kit_2.KitNo, "", custom_receive_date_trx, custom_receive_date);

            MutationPayload<SnapshotDTO> payload = await service.GenerateSnapshot(snapshotInput);
            Assert.Equal(2, payload.Payload.SnapshotCount);
            Assert.Equal(2, payload.Payload.ChangedCount);

            // no changes should reject
            snapshotInput.RunDate = baseDate.AddDays(4);
            payload = await service.GenerateSnapshot(snapshotInput);
            var expectedError = "No changes since last snapshot";
            var actualError = payload.Errors.Select(t => t.Message).FirstOrDefault();
            Assert.Equal(expectedError, actualError);
            Assert.Equal(null, payload.Payload);

            // add one change should not reject
            await AddKitTimelineEntry(TimeLineEventCode.PLAN_BUILD, kit_1.KitNo, "", plan_build_date_trx, plan_build_date);
            snapshotInput.RunDate = baseDate.AddDays(5);
            payload = await service.GenerateSnapshot(snapshotInput);

            expectedError = null;
            actualError = payload.Errors.Select(t => t.Message).FirstOrDefault();
            Assert.Equal(expectedError, actualError);
            Assert.Equal(1, payload.Payload.ChangedCount);

        }


        #region test helper methods
        public async Task<List<KitSnapshotRunDTO.Entry>> GetVehiclePartnerStatusReport(
            string plantCode,
            string engineComponentCode,
            DateTime date) {

            var snapshotInput = new KitSnapshotInput {
                PlantCode = plantCode,
                EngineComponentCode = engineComponentCode,
                RunDate = date
            };

            // initial
            var service = new KitSnapshotService(context);
            var payload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
            return payload.Entries.ToList();
        }

        public async Task<MutationPayload<KitTimelineEvent>> AddKitTimelineEntry(
            TimeLineEventCode eventType,
            string kitNo,
            string eventNote,
            DateTime trxDate,
            DateTime eventDate
        ) {
            var service = new KitService(context, trxDate, planBuildLeadTimeDays);
            var payload = await service.CreateKitTimelineEvent(new KitTimelineEventInput {
                KitNo = kitNo,
                EventType = eventType,
                EventNote = eventNote,
                EventDate = eventDate
            });
            return payload;
        }

        #endregion



    }
}