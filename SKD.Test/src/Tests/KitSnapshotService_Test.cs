namespace SKD.Test;

public class KitSnapshotServiceTest : TestBase {

    public KitSnapshotServiceTest() {
        CreateContextAndSetBaselineDatabaseData();
    }

    private void CreateContextAndSetBaselineDatabaseData() {
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data(generateLot: true, componentCodes: new List<string> { "DA", "PA", EngineComponentCode });
    }

    enum TimelineTestEvent {
        BEFORE,
        CUSTOM_RECEIVED_TRX,
        POST_CUSTOM_RECEIVED_NO_CHANGE,
        PLAN_BUILD_TRX,
        VERIFY_VIN,
        POST_PLAN_BUILD_NO_CHANGE,
        BUILD_COMPLETE_TRX,
        GATE_RELEASE_TRX,
        POST_GATE_RELEASE_TRX_NO_CHANGE,
        WHOLE_SALE_TRX,
        FINAL_2_DAYS_TRX,
        FINAL_PLUS_WHOLESALE_CUTOFF
    }

    private class ExpectedSnapshotValue {
        public TimeLineEventCode? TimeLineEventCode = null;
        public SnapshotChangeStatus? ChangeStatus = null;
        public string VIN = "";
        public string DealerCode = "";
        public string EngineSerial = "";
        public int SnapshotCount = 999;
    }

    private class TimelineEventInput {
        public int EventOnDay = 0;
        public TimeLineEventCode EventCode;
        public string DealerCode = "";

    }

    private class TestTimelineInput {
        public int Day;
        public TimelineEventInput EventInput = null;
        public ExpectedSnapshotValue Expected = new ExpectedSnapshotValue();
        public bool AddTimelineEvent = false;
    }

    [Fact]
    public async Task Can_create_full_snapshot_timeline_v2() {
        // setup
        var snapShotService = new KitSnapshotService(context);
        var kit = context.Kits
            .Include(t => t.Lot)
            .OrderBy(t => t.KitNo).First();
        var plantCode = context.Plants.Select(t => t.Code).First();
        var vin = Gen_VIN();
        var engineSerial = "GRBPA21162001726FB3Q 6007 AB3E";
        var dealerCode = await context.Dealers.Select(t => t.Code).FirstOrDefaultAsync();
        await Gen_ShipmentLot(kit.Lot.LotNo);

        var baseDate = DateTime.Now.Date;

        var testEntries = new List<TestTimelineInput> {
            new TestTimelineInput {
                Day = 1,
                Expected = new ExpectedSnapshotValue { SnapshotCount = 0 }
            },
            new TestTimelineInput {
                Day = 6,
                EventInput = new TimelineEventInput {
                    EventOnDay = 1,
                    EventCode = TimeLineEventCode.CUSTOM_RECEIVED
                },
                Expected = new ExpectedSnapshotValue {
                    TimeLineEventCode = TimeLineEventCode.CUSTOM_RECEIVED,
                    ChangeStatus = SnapshotChangeStatus.Added,
                    SnapshotCount = 1,
                    DealerCode = ""
                }
            },
            new TestTimelineInput {
                Day = 7,
                EventInput = new TimelineEventInput {
                    EventOnDay = 11,
                    EventCode = TimeLineEventCode.PLAN_BUILD
                },
                Expected = new ExpectedSnapshotValue {
                    TimeLineEventCode = TimeLineEventCode.PLAN_BUILD,
                    ChangeStatus = SnapshotChangeStatus.Changed,
                    SnapshotCount = 1,
                    DealerCode = ""
                }
            },
            new TestTimelineInput {
                Day = 8,
                Expected = new ExpectedSnapshotValue {
                    TimeLineEventCode = TimeLineEventCode.PLAN_BUILD,
                    ChangeStatus = SnapshotChangeStatus.NoChange,
                    SnapshotCount = 1,
                    DealerCode = ""
                }
            },
            new TestTimelineInput {
                Day = 9,
                EventInput = new TimelineEventInput {
                    EventOnDay = 11,
                    EventCode = TimeLineEventCode.VERIFY_VIN
                },
                Expected = new ExpectedSnapshotValue {
                    TimeLineEventCode = TimeLineEventCode.VERIFY_VIN,
                    ChangeStatus = SnapshotChangeStatus.Changed,
                    SnapshotCount = 1,
                    DealerCode = ""
                }
            },
            new TestTimelineInput {
                Day = 12,
                EventInput = new TimelineEventInput {
                    EventOnDay = 12,
                    EventCode = TimeLineEventCode.BUILD_COMPLETED
                },
                Expected = new ExpectedSnapshotValue {
                    TimeLineEventCode = TimeLineEventCode.BUILD_COMPLETED,
                    ChangeStatus = SnapshotChangeStatus.Changed,
                    SnapshotCount = 1,
                    DealerCode = "",
                    VIN = vin,
                    EngineSerial = engineSerial
                }
            },
            new TestTimelineInput {
                Day = 15,
                EventInput = new TimelineEventInput {
                    EventOnDay = 15,
                    EventCode = TimeLineEventCode.GATE_RELEASED
                },
                Expected = new ExpectedSnapshotValue {
                    TimeLineEventCode = TimeLineEventCode.GATE_RELEASED,
                    ChangeStatus = SnapshotChangeStatus.Changed,
                    SnapshotCount = 1,
                    DealerCode = "",
                    VIN = vin,
                    EngineSerial = engineSerial
                }
            },
            new TestTimelineInput {
                Day = 17,
                EventInput = new TimelineEventInput {
                    EventOnDay = 17,
                    EventCode = TimeLineEventCode.WHOLE_SALE,
                    DealerCode = dealerCode
                },
                Expected = new ExpectedSnapshotValue {
                    TimeLineEventCode = TimeLineEventCode.WHOLE_SALE,
                    ChangeStatus = SnapshotChangeStatus.Final,
                    SnapshotCount = 1,
                    DealerCode = dealerCode,
                    VIN = vin,
                    EngineSerial = engineSerial
                }
            },
        };

        var day = testEntries.OrderBy(t => t.Day).Select(t => t.Day).First();
        var endDate = testEntries.OrderBy(t => t.Day).Select(t => t.Day).Last();

        while (day <= endDate) {
            var input = testEntries.FirstOrDefault(t => t.Day == day);
            if (input != null) {
                if (input.EventInput != null) {
                    var trxDate = baseDate.AddDays(day);
                    var kitService = new KitService(context, trxDate);

                    // import VIN before VERIFY_VIN
                    if (input.EventInput.EventCode == TimeLineEventCode.VERIFY_VIN) {
                        await Gen_KitVinImport(kit.KitNo, vin);
                    }
                    // add EN component serial right after VERIFY_VIN
                    if (input.EventInput.EventCode == TimeLineEventCode.VERIFY_VIN) {
                        await Gen_KitComponentSerial(kit.KitNo, EngineComponentCode, engineSerial, "", verify: true);
                    }

                    await CreateKitTimelineEvent(
                       eventType: input.EventInput.EventCode,
                       kitNo: kit.KitNo,
                       eventNote: "",
                       dealerCode: input.EventInput.DealerCode,
                       trxDate: trxDate,
                       eventDate: baseDate.AddDays(input.EventInput.EventOnDay)
                    );

                }
                var rundDate = baseDate.AddDays(day);
                var snapshotResult = await snapShotService.GenerateSnapshot(new KitSnapshotInput {
                    PlantCode = plantCode,
                    RunDate = rundDate,
                    RejectIfNoChanges = false
                });

                // Assert
                Assert.Equal(input.Expected.SnapshotCount, snapshotResult?.Payload?.SnapshotCount);

                var kitSnapshot = await context.KitSnapshots
                    .Include(t => t.Kit).ThenInclude(t => t.Dealer)
                    .Include(t => t.KitTimeLineEventType)
                    .Include(t => t.KitSnapshotRun)
                    .Where(t => t.KitSnapshotRun.RunDate == rundDate)
                    .FirstOrDefaultAsync();

                Assert.Equal(input.Expected.ChangeStatus, kitSnapshot?.ChangeStatusCode);
                Assert.Equal(input.Expected.TimeLineEventCode, kitSnapshot?.KitTimeLineEventType.Code);
                Assert.Equal(input.Expected.DealerCode, kitSnapshot?.DealerCode ?? "");
                Assert.Equal(input.Expected.VIN, kitSnapshot?.VIN ?? "");
                Assert.Equal(input.Expected.EngineSerial, kitSnapshot?.EngineSerialNumber ?? "");
            }
            day++;
        }
    }

    [Fact(Skip = "under constuction")]
    public async Task Can_create_full_snapshot_timeline() {

        var appSettings = await ApplicationSetting.GetKnownAppSettings(context);
        var baseDate = DateTime.Now.Date;

        var testEntries = new List<(TimelineTestEvent eventType, DateTime trxDate, DateTime? eventDate)>() {
                (TimelineTestEvent.BEFORE, baseDate, baseDate ),
                (TimelineTestEvent.CUSTOM_RECEIVED_TRX, baseDate.AddDays(1), baseDate.AddDays(-appSettings.PlanBuildLeadTimeDays) ),
                (TimelineTestEvent.POST_CUSTOM_RECEIVED_NO_CHANGE, baseDate.AddDays(1), null ),
                (TimelineTestEvent.PLAN_BUILD_TRX, baseDate.AddDays(2), baseDate.AddDays(6) ),
                (TimelineTestEvent.VERIFY_VIN, baseDate.AddDays(4), baseDate.AddDays(6) ),
                (TimelineTestEvent.POST_PLAN_BUILD_NO_CHANGE, baseDate.AddDays(5), null),
                (TimelineTestEvent.BUILD_COMPLETE_TRX, baseDate.AddDays(7), baseDate.AddDays(7) ),
                (TimelineTestEvent.GATE_RELEASE_TRX, baseDate.AddDays(9), baseDate.AddDays(9) ),
                (TimelineTestEvent.WHOLE_SALE_TRX, baseDate.AddDays(11), baseDate.AddDays(11) ),
                (TimelineTestEvent.FINAL_2_DAYS_TRX, baseDate.AddDays(13), null ),
                (TimelineTestEvent.FINAL_PLUS_WHOLESALE_CUTOFF, baseDate.AddDays(13 + appSettings.WholeSaleCutoffDays), null ),
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
            RejectIfNoChanges = false,
            AllowMultipleSnapshotsPerDay = true
        };

        // 1.  empty
        snapshotInput.RunDate = testEntries.Where(t => t.eventType == TimelineTestEvent.BEFORE).First().eventDate;
        await service.GenerateSnapshot(snapshotInput);
        var snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
        Assert.Null(snapshotPayload);

        // 2.  custom received
        eventDate = testEntries.Where(t => t.eventType == TimelineTestEvent.CUSTOM_RECEIVED_TRX).First().eventDate.Value;
        snapshotInput.RunDate = testEntries.Where(t => t.eventType == TimelineTestEvent.CUSTOM_RECEIVED_TRX).First().trxDate;
        await CreateKitTimelineEvent(TimeLineEventCode.CUSTOM_RECEIVED, kit.KitNo, "", "", eventDate, eventDate.AddDays(-1));
        await service.GenerateSnapshot(snapshotInput);
        snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
        var kitSnapshot = snapshotPayload?.Entries.First(t => t.KitNo == kit.KitNo);
        Assert.Equal(TimeLineEventCode.CUSTOM_RECEIVED, kitSnapshot?.CurrentTimeLineCode);
        Assert.Equal(SnapshotChangeStatus.Added, kitSnapshot?.TxType);

        // 2.  custom no change
        eventDate = testEntries.Where(t => t.eventType == TimelineTestEvent.POST_CUSTOM_RECEIVED_NO_CHANGE).First().eventDate.Value;
        snapshotInput.RunDate = testEntries.Where(t => t.eventType == TimelineTestEvent.POST_CUSTOM_RECEIVED_NO_CHANGE).First().trxDate;
        await service.GenerateSnapshot(snapshotInput);
        var snapshotRecordCount = await context.KitSnapshots.CountAsync();
        Assert.Equal(2, snapshotRecordCount);
        snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
        kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
        Assert.Equal(1, snapshotPayload.Entries.Count);
        Assert.Equal(TimeLineEventCode.CUSTOM_RECEIVED, kitSnapshot.CurrentTimeLineCode);
        Assert.Equal(SnapshotChangeStatus.NoChange, kitSnapshot.TxType);

        // 3.  plan build
        eventDate = testEntries.Where(t => t.eventType == TimelineTestEvent.PLAN_BUILD_TRX).First().eventDate.Value;
        snapshotInput.RunDate = testEntries.Where(t => t.eventType == TimelineTestEvent.PLAN_BUILD_TRX).First().trxDate;
        await CreateKitTimelineEvent(TimeLineEventCode.PLAN_BUILD, kit.KitNo, "", "", eventDate, eventDate);
        await service.GenerateSnapshot(snapshotInput);

        snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
        kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
        Assert.Equal(TimeLineEventCode.PLAN_BUILD, kitSnapshot.CurrentTimeLineCode);
        Assert.Equal(SnapshotChangeStatus.Changed, kitSnapshot.TxType);

        // 4. post plant build no change
        eventDate = testEntries.Where(t => t.eventType == TimelineTestEvent.POST_PLAN_BUILD_NO_CHANGE).First().eventDate.Value;
        snapshotInput.RunDate = testEntries.Where(t => t.eventType == TimelineTestEvent.POST_PLAN_BUILD_NO_CHANGE).First().trxDate;
        await service.GenerateSnapshot(snapshotInput);
        snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
        kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
        Assert.Equal(TimeLineEventCode.PLAN_BUILD, kitSnapshot.CurrentTimeLineCode);
        Assert.Equal(SnapshotChangeStatus.NoChange, kitSnapshot.TxType);

        // 5. verify vin (VIN required on kit)
        eventDate = testEntries.Where(t => t.eventType == TimelineTestEvent.VERIFY_VIN).First().eventDate.Value;
        snapshotInput.RunDate = testEntries.Where(t => t.eventType == TimelineTestEvent.VERIFY_VIN).First().trxDate;

        await Gen_KitVinImport(kit.KitNo, Gen_VIN());
        await CreateKitTimelineEvent(TimeLineEventCode.VERIFY_VIN, kit.KitNo, "", "", eventDate, eventDate);
        var result = await service.GenerateSnapshot(snapshotInput);

        snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
        kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
        Assert.Equal(TimeLineEventCode.VERIFY_VIN, kitSnapshot.CurrentTimeLineCode);
        Assert.Equal(SnapshotChangeStatus.Changed, kitSnapshot.TxType);

        // 5. build completed
        eventDate = testEntries.Where(t => t.eventType == TimelineTestEvent.BUILD_COMPLETE_TRX).First().eventDate.Value;
        snapshotInput.RunDate = testEntries.Where(t => t.eventType == TimelineTestEvent.BUILD_COMPLETE_TRX).First().trxDate;
        await CreateKitTimelineEvent(TimeLineEventCode.BUILD_COMPLETED, kit.KitNo, "", "", eventDate, eventDate);
        await service.GenerateSnapshot(snapshotInput);

        snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
        kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
        Assert.Equal(TimeLineEventCode.BUILD_COMPLETED, kitSnapshot.CurrentTimeLineCode);
        Assert.Equal(SnapshotChangeStatus.Changed, kitSnapshot.TxType);

        // 5.  gate release
        eventDate = testEntries.Where(t => t.eventType == TimelineTestEvent.GATE_RELEASE_TRX).First().eventDate.Value;
        snapshotInput.RunDate = testEntries.Where(t => t.eventType == TimelineTestEvent.GATE_RELEASE_TRX).First().trxDate;
        await CreateKitTimelineEvent(TimeLineEventCode.GATE_RELEASED, kit.KitNo, "", "", eventDate, eventDate);
        await service.GenerateSnapshot(snapshotInput);

        snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
        kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
        Assert.Equal(TimeLineEventCode.GATE_RELEASED, kitSnapshot.CurrentTimeLineCode);
        Assert.Equal(SnapshotChangeStatus.Changed, kitSnapshot.TxType);

        // 6.  wholesale
        var kit_count = context.Kits.Count();
        eventDate = testEntries.Where(t => t.eventType == TimelineTestEvent.WHOLE_SALE_TRX).First().eventDate.Value;
        snapshotInput.RunDate = testEntries.Where(t => t.eventType == TimelineTestEvent.WHOLE_SALE_TRX).First().trxDate;
        await CreateKitTimelineEvent(TimeLineEventCode.WHOLE_SALE, kit.KitNo, "", dealerCode, eventDate, eventDate);
        await service.GenerateSnapshot(snapshotInput);

        snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
        kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
        Assert.Equal(TimeLineEventCode.WHOLE_SALE, kitSnapshot.CurrentTimeLineCode);
        Assert.Equal(SnapshotChangeStatus.Final, kitSnapshot.TxType);
        Assert.Equal(dealerCode, kitSnapshot.DealerCode);

        // 7.  no change ( should still be final)
        eventDate = testEntries.Where(t => t.eventType == TimelineTestEvent.FINAL_2_DAYS_TRX).First().eventDate.Value;
        snapshotInput.RunDate = testEntries.Where(t => t.eventType == TimelineTestEvent.FINAL_2_DAYS_TRX).First().trxDate;
        await service.GenerateSnapshot(snapshotInput);
        snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
        kitSnapshot = snapshotPayload.Entries.First(t => t.KitNo == kit.KitNo);
        Assert.Equal(TimeLineEventCode.WHOLE_SALE, kitSnapshot.CurrentTimeLineCode);
        Assert.Equal(SnapshotChangeStatus.Final, kitSnapshot.TxType);

        // 8.  wholesale
        eventDate = testEntries.Where(t => t.eventType == TimelineTestEvent.FINAL_PLUS_WHOLESALE_CUTOFF).First().eventDate.Value;
        snapshotInput.RunDate = testEntries.Where(t => t.eventType == TimelineTestEvent.FINAL_PLUS_WHOLESALE_CUTOFF).First().trxDate;
        await service.GenerateSnapshot(snapshotInput);
        snapshotPayload = await service.GetSnapshotRunByDate(snapshotInput.PlantCode, snapshotInput.RunDate.Value);
        Assert.Null(snapshotPayload);
    }

    [Fact]
    public async Task Kit_snapshot_dates_set_only_once() {
        // setup
        var appSetting = await ApplicationSetting.GetKnownAppSettings(context);
        var service = new KitSnapshotService(context);
        var snapshotInput = new KitSnapshotInput {
            PlantCode = context.Plants.Select(t => t.Code).First(),
        };
        var kit = context.Kits.OrderBy(t => t.KitNo).First();

        var baseDate = DateTime.Now.Date;

        var custom_receive_date = baseDate.AddDays(-1);
        var custom_receive_date_trx = baseDate;

        var plan_build_date_trx = baseDate.AddDays(1);
        var plan_build_date = custom_receive_date.AddDays(appSetting.PlanBuildLeadTimeDays);

        var new_plan_build_date_trx = baseDate.AddDays(2);
        var new_plan_build_date = custom_receive_date.AddDays(appSetting.PlanBuildLeadTimeDays + 2);

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


    record TestEntry(TimeLineEventCode eventType, int eventDateOffsetDays, int expectedSnapshotRuns, int expectedErrors);

    [Fact]
    public async Task Can_allow_or_disallow_multiple_snaphot_runs_per_day() {
        // setup

        var testSets = new List<(DateTime baseDate, bool allowMultiple, List<TestEntry> testEntries)>{
            (
                baseDate: DateTime.Now.Date.AddDays(1),
                allowMultiple: true,
                testEntries: new List<TestEntry> {
                    new TestEntry(eventType: TimeLineEventCode.CUSTOM_RECEIVED, eventDateOffsetDays: -6, expectedSnapshotRuns: 1, expectedErrors: 0),
                    new TestEntry(eventType: TimeLineEventCode.PLAN_BUILD, eventDateOffsetDays: 2, expectedSnapshotRuns: 2, expectedErrors: 0),
                }
            ),
            (
                baseDate: DateTime.Now.Date.AddDays(2),
                allowMultiple: false,
                testEntries: new List<TestEntry> {
                    new TestEntry(eventType: TimeLineEventCode.CUSTOM_RECEIVED, eventDateOffsetDays: -6, expectedSnapshotRuns: 3, expectedErrors: 0),
                    new TestEntry(eventType: TimeLineEventCode.PLAN_BUILD, eventDateOffsetDays: 2, expectedSnapshotRuns: 3, expectedErrors: 1),
                }
            ),
        };

        var service = new KitSnapshotService(context);
        var kits = context.Kits.OrderBy(t => t.KitNo).Take(2).ToList();

        var kitIndex = 0;
        foreach (var testSet in testSets) {
            var kit = kits[kitIndex++];

            var minutes = 0;
            foreach (var entry in testSet.testEntries) {

                var input = new KitSnapshotInput {
                    RunDate = testSet.baseDate.AddMinutes(++minutes),
                    PlantCode = context.Plants.Select(t => t.Code).First(),
                    RejectIfNoChanges = true,
                    AllowMultipleSnapshotsPerDay = testSet.allowMultiple
                };

                await CreateKitTimelineEvent(entry.eventType, kit.KitNo, "", "", input.RunDate.Value, testSet.baseDate.AddDays(entry.eventDateOffsetDays));
                var result = await service.GenerateSnapshot(input);

                var actualErrorCount = result.Errors.Count;
                Assert.Equal(entry.expectedErrors, actualErrorCount);

                var actualSnapshotCount = context.KitSnapshotRuns.Count();
                Assert.Equal(entry.expectedSnapshotRuns, actualSnapshotCount);
            }
        }
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
    public async Task Can_get_kit_snapshot_dates() {
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
        var kit = context.Kits.First();
        var trxDate = DateTime.Now.Date;
        var eventDate = trxDate.AddDays(-6);
        var service = new KitSnapshotService(context);

        await CreateKitTimelineEvent(TimeLineEventCode.CUSTOM_RECEIVED, kit.KitNo, "", "", trxDate, eventDate);

        var input = new KitSnapshotInput {
            RunDate = trxDate,
            PlantCode = plantCode,
            AllowMultipleSnapshotsPerDay = true
        };
        // cuustom receive, added
        var result = await service.GenerateSnapshot(input);
        var expectedErrorCount = 0;
        var actualErrorCount = result.Errors.Count();
        Assert.Equal(expectedErrorCount, actualErrorCount);

        // custom receive, no-change
        input.RunDate = trxDate.AddMinutes(10);
        result = await service.GenerateSnapshot(input);
        expectedErrorCount = 0;
        actualErrorCount = result.Errors.Count();
        Assert.Equal(expectedErrorCount, actualErrorCount);

        // custom receive, no-change
        input.RunDate = trxDate.AddMinutes(20);
        result = await service.GenerateSnapshot(input);
        expectedErrorCount = 1;
        actualErrorCount = result.Errors.Count();
        Assert.Equal(expectedErrorCount, actualErrorCount);

        var expectedErrorMessage = "No changes since last snapshot";
        var actualErrorMessage = result.Errors.Select(t => t.Message).FirstOrDefault();
        Assert.StartsWith(expectedErrorMessage, actualErrorMessage);
    }

    [Fact]
    public async Task Can_import_partner_status_ackknowledgement() {
        // setup
        var plantCode = context.Plants.Select(t => t.Code).First();
        var kit = context.Kits.First();
        var trxDate = DateTime.Now.Date;
        var eventDate = trxDate.AddDays(-6);
        var service = new KitSnapshotService(context);

        await CreateKitTimelineEvent(TimeLineEventCode.CUSTOM_RECEIVED, kit.KitNo, "", "", trxDate, eventDate);

        var snapshotInput = new KitSnapshotInput {
            RunDate = trxDate,
            PlantCode = plantCode,
            AllowMultipleSnapshotsPerDay = true
        };
        // cuustom receive, added
        var result = await service.GenerateSnapshot(snapshotInput);

        var ackInput = new PartnerStatusAckDTO {
            Sequence = result.Payload.Sequence,
            TotalProcessed = result.Payload.SnapshotCount,
            TotalAccepted = result.Payload.SnapshotCount,
            TotalRejected = 0,
            FileDate = DateTime.Now.Date.ToString("yyyy-MM-dd")
        };

        var importResult = await service.ImportPartnerStatusAck(ackInput);

        Assert.True(0 == importResult.Errors.Count);

        var snapshot = await context.KitSnapshotRuns
            .Include(t => t.PartnerStatusAck)
            .FirstAsync(t => t.Sequence == result.Payload.Sequence);

        Assert.Null(snapshot.RemovedAt);
        Assert.Equal(ackInput.TotalProcessed, snapshot.PartnerStatusAck.TotalProcessed);
        Assert.Equal(ackInput.TotalAccepted, snapshot.PartnerStatusAck.TotalAccepted);
        Assert.Equal(ackInput.TotalRejected, snapshot.PartnerStatusAck.TotalRejected);
    }

    [Fact]
    public async Task Will_reject_if_prior_snapshot_acknowledgment_required() {
        // setup
        var plantCode = context.Plants.Select(t => t.Code).First();
        var kit = context.Kits.First();
        var trxDate = DateTime.Now.Date;
        var eventDate = trxDate.AddDays(-6);
        var service = new KitSnapshotService(context);

        await CreateKitTimelineEvent(TimeLineEventCode.CUSTOM_RECEIVED, kit.KitNo, "", "", trxDate, eventDate);

        var snapshotInput = new KitSnapshotInput {
            RunDate = trxDate,
            PlantCode = plantCode,
            RejectIfNoChanges = false,
            RejectIfPriorSnapshotNotAcknowledged = true,
            AllowMultipleSnapshotsPerDay = true
        };
        // generate first snapshot
        var result1 = await service.GenerateSnapshot(snapshotInput);
        Assert.False(result1.Errors.Any());

        // try to generate second snapshot before importing acknowledment
        var result2 = await service.GenerateSnapshot(snapshotInput);
        Assert.True(result2.Errors.Any());

        // Import acknowledgment

        var ackInput = new PartnerStatusAckDTO {
            Sequence = result1.Payload.Sequence,
            TotalProcessed = result1.Payload.SnapshotCount,
            TotalAccepted = result1.Payload.SnapshotCount,
            TotalRejected = 0,
            FileDate = DateTime.Now.Date.ToString("yyyy-MM-dd")
        };

        var importResult = await service.ImportPartnerStatusAck(ackInput);
        Assert.False(importResult.Errors.Any());

        // try generating snapshot again.  This time it should succeeed
        snapshotInput.RunDate = trxDate.AddDays(1);
        var result3 = await service.GenerateSnapshot(snapshotInput);
        Assert.False(result3.Errors.Any());
    }

    #region test helper methods
    public async Task<List<KitSnapshotRunDTO.Entry>> GetVehiclePartnerStatusReport(
        string plantCode,
        string engineComponentCode,
        DateTime date) {

        var snapshotInput = new KitSnapshotInput {
            PlantCode = plantCode,
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

        var service = new KitService(context, trxDate);
        var result = await service.CreateKitTimelineEvent(new KitTimelineEventInput {
            KitNo = kitNo,
            EventCode = eventType,
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

        return result;
    }


    #endregion
}
