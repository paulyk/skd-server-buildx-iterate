namespace SKD.Test;

public class KitSnapshotServiceTest : TestBase {

    readonly string engineCode = "EN";
    readonly int wholeSateCutOffDays = 7;
    readonly int planBuildLeadTimeDays = 2;

    public KitSnapshotServiceTest() {
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data(generateLot: true, componentCodes: new List<string> { "DA", "PA", engineCode });
    }

    
    enum TimelineTestEvent {
        BEFORE,
        CUSTOM_RECEIVED_TRX,
        POST_CUSTOM_RECEIVED_NO_CHANGE,
        PLAN_BUILD_TRX,
        POST_PLAN_BUILD_NO_CHANGE,
        BUILD_COMPLETE_TRX,
        GATE_RELEASE_TRX,
        POST_GATE_RELEASE_TRX_NO_CHANGE,
        WHOLE_SALE_TRX,
        FINAL_2_DAYS_TRX,
        FINAL_PLUS_WHOLESALE_CUTOFF
    }

    private record TimelineEventSet(
        string description,
        TimeLineEventCode? eventCode,
        DateTime? runDate,
        DateTime? eventDate,
        string dealerCode,
        // expected kit snapshot result
        TimeLineEventCode? expected_EventCode,
        SnapshotChangeStatus? expectedChangeStatus,
        string expectedDealerCode
    );

    [Fact]
    public async Task Can_create_full_snapshot_timeline_v2() {
        var snapShotService = new KitSnapshotService(context);
        var kit = context.Kits.Include(t => t.Lot).OrderBy(t => t.KitNo).First();
        var plantCode = context.Plants.Select(t => t.Code).First();
        var dealerCode = await context.Dealers.Select(t => t.Code).FirstOrDefaultAsync();
        await Gen_ShipmentLot(kit.Lot.LotNo);

        var runDate = DateTime.Now.Date;
        DateTime eventDate = runDate;

        var testEntries = new List<TimelineEventSet> {
            new TimelineEventSet("Custom receive", TimeLineEventCode.CUSTOM_RECEIVED,runDate, runDate.AddDays(-6), dealerCode, TimeLineEventCode.CUSTOM_RECEIVED, SnapshotChangeStatus.Added, null ),
            new TimelineEventSet("Plan build", TimeLineEventCode.PLAN_BUILD,runDate.AddDays(1), runDate.AddDays(3), dealerCode, TimeLineEventCode.PLAN_BUILD, SnapshotChangeStatus.Changed, null ),
            new TimelineEventSet("Build complete", TimeLineEventCode.BUILD_COMPLETED,runDate.AddDays(4), runDate.AddDays(4), dealerCode, TimeLineEventCode.BUILD_COMPLETED, SnapshotChangeStatus.Changed, null ),
            new TimelineEventSet("Gate Release ", TimeLineEventCode.GATE_RELEASED,runDate.AddDays(8), runDate.AddDays(8), dealerCode, TimeLineEventCode.GATE_RELEASED, SnapshotChangeStatus.Changed, null ),
            new TimelineEventSet("Wholesalte ", TimeLineEventCode.WHOLE_SALE,runDate.AddDays(9), runDate.AddDays(9), dealerCode, TimeLineEventCode.WHOLE_SALE, SnapshotChangeStatus.Final, dealerCode ),
        };

        foreach (var entry in testEntries) {
            var kitService = new KitService(context, entry.runDate.Value, planBuildLeadTimeDays);

            await CreateKitTimelineEvent(
                eventType: entry.eventCode.Value,
                kitNo: kit.KitNo,
                eventNote: "",
                dealerCode: entry.dealerCode,
                trxDate: entry.runDate.Value,
                eventDate: entry.eventDate.Value
            );

            var result = await snapShotService.GenerateSnapshot(new KitSnapshotInput {
                PlantCode = plantCode,
                RunDate = entry.runDate.Value,
                EngineComponentCode = engineCode
            });

            var kitSnapshot = await context.KitSnapshots
                .Include(t => t.Kit).ThenInclude(t => t.Dealer)
                .Include(t => t.KitTimeLineEventType)
                .Include(t => t.KitSnapshotRun)
                .Where(t => t.KitSnapshotRun.RunDate == entry.runDate.Value)
                .FirstOrDefaultAsync();

            Assert.Equal(entry.expectedChangeStatus, kitSnapshot.ChangeStatusCode);
            Assert.Equal(entry.eventCode, kitSnapshot.KitTimeLineEventType.Code);
            Assert.Equal(entry.expectedDealerCode, kitSnapshot.DealerCode);
        }
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
                (TimelineTestEvent.POST_GATE_RELEASE_TRX_NO_CHANGE, baseDate.AddDays(9) ),
                (TimelineTestEvent.WHOLE_SALE_TRX, baseDate.AddDays(10) ),
                (TimelineTestEvent.FINAL_2_DAYS_TRX, baseDate.AddDays(12) ),
                (TimelineTestEvent.FINAL_PLUS_WHOLESALE_CUTOFF, baseDate.AddDays(10 + wholeSateCutOffDays) ),
            };

        // setup
        var service = new KitSnapshotService(context);
        var kit = context.Kits.OrderBy(t => t.KitNo).First();
        var dealerCode = await context.Dealers.Select(t => t.Code).FirstOrDefaultAsync();
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
        await CreateKitTimelineEvent(TimeLineEventCode.CUSTOM_RECEIVED, kit.KitNo, "", "", eventDate, eventDate.AddDays(-1));
        snapshotInput.RunDate = eventDate;
        await service.GenerateSnapshot(snapshotInput);
        snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
        var kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
        Assert.Equal(TimeLineEventCode.CUSTOM_RECEIVED, kitSnapshot.CurrentTimeLineCode);
        Assert.Equal(SnapshotChangeStatus.Added, kitSnapshot.TxType);

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
        Assert.Equal(SnapshotChangeStatus.NoChange, kitSnapshot.TxType);

        // 3.  plan build
        eventDate = dates.Where(t => t.eventType == TimelineTestEvent.PLAN_BUILD_TRX).First().date;
        snapshotInput.RunDate = eventDate;
        await CreateKitTimelineEvent(TimeLineEventCode.PLAN_BUILD, kit.KitNo, "", "", eventDate, eventDate);
        await service.GenerateSnapshot(snapshotInput);

        snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
        kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
        Assert.Equal(TimeLineEventCode.PLAN_BUILD, kitSnapshot.CurrentTimeLineCode);
        Assert.Equal(SnapshotChangeStatus.Changed, kitSnapshot.TxType);

        // 4. post plant build no change
        eventDate = dates.Where(t => t.eventType == TimelineTestEvent.POST_PLAN_BUILD_NO_CHANGE).First().date;
        snapshotInput.RunDate = eventDate;
        await service.GenerateSnapshot(snapshotInput);
        snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
        kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
        Assert.Equal(TimeLineEventCode.PLAN_BUILD, kitSnapshot.CurrentTimeLineCode);
        Assert.Equal(SnapshotChangeStatus.NoChange, kitSnapshot.TxType);

        // 5. build completed
        eventDate = dates.Where(t => t.eventType == TimelineTestEvent.BUILD_COMPLETE_TRX).First().date;
        snapshotInput.RunDate = eventDate;
        await CreateKitTimelineEvent(TimeLineEventCode.BUILD_COMPLETED, kit.KitNo, "", "", eventDate, eventDate);
        await service.GenerateSnapshot(snapshotInput);

        snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
        kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
        Assert.Equal(TimeLineEventCode.BUILD_COMPLETED, kitSnapshot.CurrentTimeLineCode);
        Assert.Equal(SnapshotChangeStatus.Changed, kitSnapshot.TxType);

        // 5.  gate release
        eventDate = dates.Where(t => t.eventType == TimelineTestEvent.GATE_RELEASE_TRX).First().date;
        snapshotInput.RunDate = eventDate;
        await CreateKitTimelineEvent(TimeLineEventCode.GATE_RELEASED, kit.KitNo, "", "", eventDate, eventDate);
        await service.GenerateSnapshot(snapshotInput);

        snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
        kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
        Assert.Equal(TimeLineEventCode.GATE_RELEASED, kitSnapshot.CurrentTimeLineCode);
        Assert.Equal(SnapshotChangeStatus.Changed, kitSnapshot.TxType);

        // 5.  post gate release
        eventDate = dates.Where(t => t.eventType == TimelineTestEvent.POST_GATE_RELEASE_TRX_NO_CHANGE).First().date;
        snapshotInput.RunDate = eventDate;
        await service.GenerateSnapshot(snapshotInput);

        snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
        kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
        Assert.Equal(TimeLineEventCode.GATE_RELEASED, kitSnapshot.CurrentTimeLineCode);
        Assert.Equal(SnapshotChangeStatus.NoChange, kitSnapshot.TxType);

        // 6.  wholesale
        var kit_count = context.Kits.Count();
        eventDate = dates.Where(t => t.eventType == TimelineTestEvent.WHOLE_SALE_TRX).First().date;
        snapshotInput.RunDate = eventDate;
        await CreateKitTimelineEvent(TimeLineEventCode.WHOLE_SALE, kit.KitNo, "", dealerCode, eventDate, eventDate);
        await service.GenerateSnapshot(snapshotInput);

        snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
        kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
        Assert.Equal(TimeLineEventCode.WHOLE_SALE, kitSnapshot.CurrentTimeLineCode);
        Assert.Equal(SnapshotChangeStatus.Final, kitSnapshot.TxType);
        Assert.Equal(dealerCode, kitSnapshot.DealerCode);

        // 7.  no change ( should still be final)
        eventDate = dates.Where(t => t.eventType == TimelineTestEvent.FINAL_2_DAYS_TRX).First().date;
        snapshotInput.RunDate = eventDate;
        await service.GenerateSnapshot(snapshotInput);
        snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
        kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
        Assert.Equal(TimeLineEventCode.WHOLE_SALE, kitSnapshot.CurrentTimeLineCode);
        Assert.Equal(SnapshotChangeStatus.Final, kitSnapshot.TxType);

        // 8.  wholesale
        eventDate = dates.Where(t => t.eventType == TimelineTestEvent.FINAL_PLUS_WHOLESALE_CUTOFF).First().date;
        snapshotInput.RunDate = eventDate;
        await service.GenerateSnapshot(snapshotInput);
        snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
        Assert.Null(snapshotPayload);
    }

    [Fact]
    public async Task Kit_snapshot_dates_set_only_once() {
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
        await CreateKitTimelineEvent(TimeLineEventCode.CUSTOM_RECEIVED, kit.KitNo, "note", "", custom_receive_date_trx, custom_receive_date);
        snapshotInput.RunDate = custom_receive_date_trx;
        var result = await service.GenerateSnapshot(snapshotInput);
        var snapshotRun = await service.GetSnapshotRunBySequence(snapshotInput.PlantCode, result.Payload.Sequence);
        var kitSnapshot = snapshotRun.Entries.First(t => t.KitNo == kit.KitNo);
        Assert.Equal(1, snapshotRun.Entries.Count);
        Assert.Equal(TimeLineEventCode.CUSTOM_RECEIVED, kitSnapshot.CurrentTimeLineCode);
        Assert.Equal(SnapshotChangeStatus.Added, kitSnapshot.TxType);

        // 1.  plan build 
        await CreateKitTimelineEvent(TimeLineEventCode.PLAN_BUILD, kit.KitNo, "note", "", plan_build_date_trx, plan_build_date);
        snapshotInput.RunDate = plan_build_date_trx;
        result = await service.GenerateSnapshot(snapshotInput);
        snapshotRun = await service.GetSnapshotRunBySequence(snapshotInput.PlantCode, result.Payload.Sequence);
        kitSnapshot = snapshotRun.Entries.First(t => t.KitNo == kit.KitNo);
        Assert.Equal(1, snapshotRun.Entries.Count);
        Assert.Equal(TimeLineEventCode.PLAN_BUILD, kitSnapshot.CurrentTimeLineCode);
        Assert.Equal(plan_build_date, kitSnapshot.PlanBuild);
        Assert.Equal(plan_build_date, kitSnapshot.OriginalPlanBuild);
        Assert.Equal(SnapshotChangeStatus.Changed, kitSnapshot.TxType);

        // 1. edit plan build date does not change kitSnapshot planBuild or OriginalPlanbuild
        var orignalPlanBuild = kitSnapshot.OriginalPlanBuild;
        var planBuild = kitSnapshot.PlanBuild;
        await CreateKitTimelineEvent(TimeLineEventCode.PLAN_BUILD, kit.KitNo, "note", "", new_plan_build_date_trx, new_plan_build_date);
        snapshotRun = await service.GetSnapshotRunBySequence(snapshotInput.PlantCode, result.Payload.Sequence);
        var kitSnapshot_2 = snapshotRun.Entries.First(t => t.KitNo == kit.KitNo);
        Assert.Equal(orignalPlanBuild, kitSnapshot_2.OriginalPlanBuild);
        Assert.Equal(planBuild, kitSnapshot_2.PlanBuild);
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
        await CreateKitTimelineEvent(TimeLineEventCode.CUSTOM_RECEIVED, kit.KitNo, "", "", first_trx_date, base_date);
        await CreateKitTimelineEvent(TimeLineEventCode.PLAN_BUILD, kit.KitNo, "", "", first_trx_date, plan_build_date);
        await CreateKitTimelineEvent(TimeLineEventCode.BUILD_COMPLETED, kit.KitNo, "", "", first_trx_date, build_completed_date);

        var snapShotService = new KitSnapshotService(context);
        var partnerStatusBuilder = new PartnerStatusBuilder(context);
        var detailLineParser = new FlatFileLine<PartnerStatusLayout.Detail>();

        MutationResult<SnapshotDTO> snapShotPayload = null;
        KitSnapshotRunDTO snapshotRun = null;
        int snapshotCount = 0;
        KitSnapshotRunDTO.Entry kitSnapshot = null;

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
            Assert.Equal(SnapshotChangeStatus.Added, kitSnapshot.TxType);
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
            Assert.Equal(SnapshotChangeStatus.Changed, kitSnapshot.TxType);

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
            Assert.Equal(SnapshotChangeStatus.Changed, kitSnapshot.TxType);

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
            Assert.Equal(SnapshotChangeStatus.NoChange, kitSnapshot.TxType);

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

        await CreateKitTimelineEvent(TimeLineEventCode.CUSTOM_RECEIVED, kit.KitNo, "", "", custom_receive_date_trx, custom_receive_date);
        var service = new KitSnapshotService(context);

        var snapshotInput = new KitSnapshotInput {
            RunDate = custom_receive_date_trx,
            PlantCode = context.Plants.Select(t => t.Code).First(),
            EngineComponentCode = engineCode
        };
        var result = await service.GenerateSnapshot(snapshotInput);
        var snapshots_count = context.KitSnapshots.Count();
        Assert.Equal(1, snapshots_count);

        // test with same runDate
        result = await service.GenerateSnapshot(snapshotInput);

        var errorCount = result.Errors.Count;
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
        await CreateKitTimelineEvent(TimeLineEventCode.CUSTOM_RECEIVED, kit.KitNo, "", "", custom_receive_date_trx, custom_receive_date);

        snapshotInput.RunDate = run_date_1;
        var result = await service.GenerateSnapshot(snapshotInput);
        Assert.Equal(1, result.Payload.Sequence);

        snapshotInput.RunDate = run_date_2;
        result = await service.GenerateSnapshot(snapshotInput);
        Assert.Equal(2, result.Payload.Sequence);

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
        await CreateKitTimelineEvent(TimeLineEventCode.CUSTOM_RECEIVED, kit.KitNo, "", "", custom_receive_date_trx, custom_receive_date);

        snapshotInput.RunDate = run_date_1;
        result = await service.GenerateSnapshot(snapshotInput);
        Assert.Equal(1, result.Payload.Sequence);

        snapshotInput.RunDate = run_date_2;
        result = await service.GenerateSnapshot(snapshotInput);
        Assert.Equal(2, result.Payload.Sequence);


        // total snapshot run entries
        var kit_snapshot_runs = await context.KitSnapshotRuns.CountAsync();
        Assert.Equal(4, kit_snapshot_runs);
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
        await CreateKitTimelineEvent(TimeLineEventCode.CUSTOM_RECEIVED, kit_1.KitNo, "", "", custom_receive_date_trx, custom_receive_date);
        await CreateKitTimelineEvent(TimeLineEventCode.CUSTOM_RECEIVED, kit_2.KitNo, "", "", custom_receive_date_trx, custom_receive_date);
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

        await CreateKitTimelineEvent(TimeLineEventCode.CUSTOM_RECEIVED, kit_1.KitNo, "", "", custom_receive_date_trx, custom_receive_date);
        await CreateKitTimelineEvent(TimeLineEventCode.CUSTOM_RECEIVED, kit_2.KitNo, "", "", custom_receive_date_trx, custom_receive_date);

        MutationResult<SnapshotDTO> result = await service.GenerateSnapshot(snapshotInput);
        Assert.Equal(2, result.Payload.SnapshotCount);
        Assert.Equal(2, result.Payload.ChangedCount);

        // no changes should reject
        snapshotInput.RunDate = baseDate.AddDays(4);
        var result_2 = await service.GenerateSnapshot(snapshotInput);
        var expectedError = "No changes since last snapshot";
        var actualError = result_2.Errors.Select(t => t.Message).FirstOrDefault();
        Assert.Equal(expectedError, actualError);

        // add one change should not reject
        await CreateKitTimelineEvent(TimeLineEventCode.PLAN_BUILD, kit_1.KitNo, "", "", plan_build_date_trx, plan_build_date);
        snapshotInput.RunDate = baseDate.AddDays(5);
        var result_3 = await service.GenerateSnapshot(snapshotInput);

        expectedError = null;
        actualError = result_3.Errors.Select(t => t.Message).FirstOrDefault();
        Assert.Equal(expectedError, actualError);
        Assert.Equal(1, result_3.Payload.ChangedCount);
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
        var result = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
        return result.Entries.ToList();
    }

    public async Task<MutationResult<KitTimelineEvent>> CreateKitTimelineEvent(
        TimeLineEventCode eventType,
        string kitNo,
        string eventNote,
        string dealerCode,
        DateTime trxDate,
        DateTime eventDate
    ) {
        // ensure shipment lot
        await Gen_ShipmentLot_ForKit(kitNo);

        var service = new KitService(context, trxDate, planBuildLeadTimeDays);
        var result = await service.CreateKitTimelineEvent(new KitTimelineEventInput {
            KitNo = kitNo,
            EventType = eventType,
            EventNote = eventNote,
            EventDate = eventDate,
            DealerCode = dealerCode
        });

        // WHOLESLAE cutoff date rules are based on kit_timeline_event createdAt
        // To ensure those tests pass we update the createAt to match the eventDate
        var kte = await context.KitTimelineEvents
            .OrderByDescending(t => t.CreatedAt)
            .Where(t => t.RemovedAt == null)
            .Where(t => t.Kit.KitNo == kitNo && t.EventType.Code == eventType)
            .FirstAsync();
        
        kte.CreatedAt = eventDate;
        await context.SaveChangesAsync();
        
        //
        return result;
    }

    #endregion
}
