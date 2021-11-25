namespace SKD.Test;
public class KitServiceTest : TestBase {

    readonly int planBuildLeadTimeDays = 2;
    public KitServiceTest() {
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data();
    }

    [Fact]
    public async Task Can_import_vin() {
        // setup
        var lot = context.Lots.First();
        var plant = context.Plants.First();
        var partnerPlantCode = Gen_PartnerPLantCode();
        var sequence = 1;

        var input = new VinFile {
            PlantCode = plant.Code,
            PartnerPlantCode = partnerPlantCode,
            Sequence = sequence,
            Kits = lot.Kits.Select(t => new VinFile.VinFileKit {
                LotNo = lot.LotNo,
                KitNo = t.KitNo,
                VIN = Gen_VIN()
            }).ToList()
        };

        // test
        var kits = await context.Kits.Where(t => t.Lot.LotNo == lot.LotNo).ToListAsync();
        var lot_kit_count = kits.Count;
        var with_vin_count = kits.Count(t => !String.IsNullOrWhiteSpace(t.VIN));
        Assert.True(0 == with_vin_count);

        var service = new KitService(context, DateTime.Now, planBuildLeadTimeDays);
        var payload = await service.ImportVIN(input);

        // assert
        var kitVinImport = await context.KitVinImports.FirstOrDefaultAsync(t => t.Plant.Code == input.PlantCode && t.Sequence == input.Sequence);
        Assert.NotNull(kitVinImport);

        var kitvin_count = await context.KitVins.CountAsync(t => t.KitVinImportId == kitVinImport.Id);
        var expected_count = input.Kits.Count;
        Assert.True(expected_count == kitvin_count);

        // partner code
        Assert.Equal(input.PartnerPlantCode, kitVinImport.PartnerPlantCode);

        kits = await context.Kits.Where(t => t.Lot.LotNo == lot.LotNo).ToListAsync();
        with_vin_count = kits.Count(t => t.VIN != "");
        Assert.Equal(lot_kit_count, with_vin_count);
    }

    [Fact]
    public async Task Cannot_re_import_vin_file_with_same_plant_and_sequence() {
        // setup
        var lot = context.Lots.First();
        var plant = context.Plants.First();
        var partnerPlantCode = Gen_PartnerPLantCode();
        var sequence = 1;

        var input = new VinFile {
            PlantCode = plant.Code,
            PartnerPlantCode = partnerPlantCode,
            Sequence = sequence,
            Kits = lot.Kits.Select(t => new VinFile.VinFileKit {
                LotNo = lot.LotNo,
                KitNo = t.KitNo,
                VIN = Gen_VIN()
            }).ToList()
        };

        // test
        var service = new KitService(context, DateTime.Now, planBuildLeadTimeDays);
        await service.ImportVIN(input);
        var payload = await service.ImportVIN(input);

        // assert
        var expectedErrorMessage = "Already imported plant - sequence";
        var actualErrorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
        Assert.Equal(expectedErrorMessage, actualErrorMessage.Substring(0, expectedErrorMessage.Length));
    }

    [Fact]
    public async Task VIN_Import_can_change_vin() {
        // setup
        var lot = context.Lots.First();
        var plant = context.Plants.First();
        var partnerPlantCode = Gen_PartnerPLantCode();
        var sequence = 1;

        var firstInput = new VinFile {
            PlantCode = plant.Code,
            PartnerPlantCode = partnerPlantCode,
            Sequence = sequence,
            Kits = lot.Kits.Select(t => new VinFile.VinFileKit {
                LotNo = lot.LotNo,
                KitNo = t.KitNo,
                VIN = Gen_VIN()
            }).ToList()
        };

        var newVin = Gen_VIN();
        var change_vin_kitNo = firstInput.Kits.Select(t => t.KitNo).First();

        var secontInput = new VinFile {
            PlantCode = firstInput.PlantCode,
            PartnerPlantCode = firstInput.PartnerPlantCode,
            Sequence = firstInput.Sequence + 1,
            Kits = firstInput.Kits.Select(t => new VinFile.VinFileKit {
                LotNo = lot.LotNo,
                KitNo = t.KitNo,
                VIN = t.KitNo == change_vin_kitNo ? newVin : t.VIN
            }).ToList()
        };

        // first import kit vinds
        var service = new KitService(context, DateTime.Now, planBuildLeadTimeDays);
        await service.ImportVIN(firstInput);
        var kitVinCount_before = await context.KitVins.CountAsync();

        // now import with one VIN change
        var paylaod = service.ImportVIN(secontInput);

        var kitVinCount_after = await context.KitVins.CountAsync();
        Assert.Equal(kitVinCount_before + 1, kitVinCount_after);

        // assert vin match for each kit
        foreach (var inputKit in secontInput.Kits.ToList()) {
            var kit = await context.Kits
                .Include(t => t.KitVins)
                .FirstOrDefaultAsync(t => t.KitNo == inputKit.KitNo);
            Assert.Equal(inputKit.VIN, kit.VIN);

            var kitVins_count = kit.KitVins.Count();
            if (inputKit.VIN == newVin) {
                Assert.Equal(2, kitVins_count);
            } else {
                Assert.Equal(1, kitVins_count);
            }
        }
    }

    [Fact]
    public async Task Cannot_import_kit_vins_if_kits_not_found() {
        // setup
        var lot = context.Lots.First();
        var plant = context.Plants.First();
        var partnerPlantCode = Gen_PartnerPLantCode();
        var sequence = 3;

        var kitVinDto = new VinFile {
            PlantCode = plant.Code,
            PartnerPlantCode = partnerPlantCode,
            Sequence = sequence,
            Kits = lot.Kits.Select(t => new VinFile.VinFileKit {
                LotNo = lot.LotNo,
                KitNo = Gen_KitNo(), // generate a kit not thats different
                VIN = Gen_VIN()
            }).ToList()
        };

        // test
        var service = new KitService(context, DateTime.Now, planBuildLeadTimeDays);
        var payload = await service.ImportVIN(kitVinDto);

        // assert
        var expectedError = "kit numbers not found";
        var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
        errorMessage = errorMessage.Substring(0, expectedError.Length);
        Assert.Equal(expectedError, errorMessage);
    }

    [Fact]
    public async Task Cannot_import_vins_with_duplicate_KitNo_in_payload() {
        // setup
        var lot = context.Lots.First();
        var plant = context.Plants.First();
        var partnerPlantCode = Gen_PartnerPLantCode();
        var sequence = 3;

        var lotVehicles = lot.Kits.ToList();

        var input = new VinFile {
            PlantCode = plant.Code,
            PartnerPlantCode = partnerPlantCode,
            Sequence = sequence,
        };

        input.Kits = new List<VinFile.VinFileKit>() {
                new VinFile.VinFileKit {LotNo = lot.LotNo, KitNo = lotVehicles[0].KitNo, VIN = Gen_VIN() },
                new VinFile.VinFileKit {LotNo = lot.LotNo, KitNo = lotVehicles[1].KitNo, VIN = Gen_VIN() },
                new VinFile.VinFileKit {LotNo = lot.LotNo, KitNo = lotVehicles[2].KitNo, VIN = Gen_VIN() },
                new VinFile.VinFileKit {LotNo = lot.LotNo, KitNo = lotVehicles[3].KitNo, VIN = Gen_VIN() },
                // duplicate kits
                new VinFile.VinFileKit {LotNo = lot.LotNo, KitNo = lotVehicles[5].KitNo, VIN = Gen_VIN() },
                new VinFile.VinFileKit {LotNo = lot.LotNo, KitNo = lotVehicles[5].KitNo, VIN = Gen_VIN() },
            };
        // test
        var service = new KitService(context, DateTime.Now, planBuildLeadTimeDays);
        var payload_2 = await service.ImportVIN(input);

        // assert
        var expectedError = "duplicate kitNo(s) in payload";
        var errorMessage = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
        errorMessage = errorMessage.Substring(0, expectedError.Length);
        Assert.Equal(expectedError, errorMessage);
    }

    [Fact]
    public async Task Can_create_kit_timeline_events() {
        var baseDate = DateTime.Now.Date;
        // setup        
        var dealerCode = await context.Dealers.Select(t => t.Code).FirstOrDefaultAsync();
        var timelineEvents = new List<(TimeLineEventCode eventType, DateTime eventDate)>() {
                (TimeLineEventCode.CUSTOM_RECEIVED, baseDate.AddDays(-5)),
                (TimeLineEventCode.PLAN_BUILD, baseDate.AddDays(2)),
                (TimeLineEventCode.BUILD_COMPLETED, baseDate.AddDays(5)),
                (TimeLineEventCode.GATE_RELEASED, baseDate.AddDays(10)),
                (TimeLineEventCode.WHOLE_SALE, baseDate.AddDays(12)),
            };

        // test
        var kit = context.Kits.First();
        await Gen_ShipmentLot_ForKit(kit.KitNo);

        var service = new KitService(context, baseDate, planBuildLeadTimeDays);
        var payloads = new List<MutationPayload<KitTimelineEvent>>();

        var before_count = context.KitTimelineEvents.Count();

        foreach (var entry in timelineEvents) {
            var dto = new KitTimelineEventInput {
                KitNo = kit.KitNo,
                EventType = entry.eventType,
                EventDate = entry.eventDate,
                DealerCode = dealerCode
            };
            var payload = await service.CreateKitTimelineEvent(dto);
            payloads.Add(payload);
        }

        // assert
        var after_count = context.KitTimelineEvents.Count();
        Assert.Equal(0, before_count);
        Assert.Equal(timelineEvents.Count, after_count);
    }

    [Fact]
    public async Task Error_if_custom_receive_date_greater_than_current_date() {

        // test
        var kit = context.Kits.First();
        await Gen_ShipmentLot_ForKit(kit.KitNo);

        var currentDate = DateTime.Now.Date;
        var service = new KitService(context, currentDate, planBuildLeadTimeDays);

        var input_1 = new KitTimelineEventInput {
            KitNo = kit.KitNo,
            EventType = TimeLineEventCode.CUSTOM_RECEIVED,
            EventDate = currentDate
        };

        var input_2 = new KitTimelineEventInput {
            KitNo = kit.KitNo,
            EventType = TimeLineEventCode.CUSTOM_RECEIVED,
            EventDate = currentDate.AddDays(-1)
        };

        // test
        var payload_1 = await service.CreateKitTimelineEvent(input_1);
        var payload_2 = await service.CreateKitTimelineEvent(input_2);

        // assert
        var expectedError = "custom received date must be before current date";
        var actualMessage = payload_1.Errors.Select(t => t.Message).FirstOrDefault();
        Assert.Equal(expectedError, actualMessage);

        var errorCount = payload_2.Errors.Count;
        Assert.Equal(0, errorCount);
    }

    [Fact]
    public async Task Cannot_create_kit_timeline_events_out_of_sequence() {
        // setup
        var baseDate = DateTime.Now.Date;
        var timelineEvents = new List<(TimeLineEventCode eventType, DateTime trxDate, DateTime eventDate)>() {
                (TimeLineEventCode.CUSTOM_RECEIVED, baseDate.AddDays(1),  baseDate.AddDays(6)),
                (TimeLineEventCode.BUILD_COMPLETED, baseDate.AddDays(2), baseDate.AddDays(2)),
            };

        var kit = context.Kits.First();
        await Gen_ShipmentLot_ForKit(kit.KitNo);

        // test
        KitService service = null;
        var payloads = new List<MutationPayload<KitTimelineEvent>>();

        foreach (var (eventType, eventDate, trxDate) in timelineEvents) {
            var input = new KitTimelineEventInput {
                KitNo = kit.KitNo,
                EventType = eventType,
                EventDate = eventDate,
            };
            service = new KitService(context, trxDate, planBuildLeadTimeDays);
            var payload = await service.CreateKitTimelineEvent(input);
            payloads.Add(payload);
        }

        var lastPayload = payloads[1];

        // assert
        var expectedMessage = "prior timeline event(s) missing";
        var actualMessage = lastPayload.Errors.Select(t => t.Message).FirstOrDefault();
        Assert.Equal(expectedMessage, actualMessage.Substring(0, expectedMessage.Length));
    }

    [Fact]
    public async Task Create_kit_timeline_event_with_note() {
        // setup
        var kit = context.Kits.First();
        await Gen_ShipmentLot_ForKit(kit.KitNo);

        var dealer = await context.Dealers.FirstOrDefaultAsync();
        var eventNote = Util.RandomString(15);
        var baseDate = DateTime.Now.Date;
        var timelineEventItems = new List<(TimeLineEventCode eventType, DateTime trxDate, DateTime eventDate, string eventNode, string dealerCode)>() {
                (TimeLineEventCode.CUSTOM_RECEIVED, baseDate.AddDays(2), baseDate.AddDays(1) , eventNote, null),
                (TimeLineEventCode.PLAN_BUILD, baseDate.AddDays(3), baseDate.AddDays(5), eventNote, null),
                (TimeLineEventCode.BUILD_COMPLETED, baseDate.AddDays(8), baseDate.AddDays(8), eventNote, null),
                (TimeLineEventCode.GATE_RELEASED, baseDate.AddDays(10), baseDate.AddDays(10), eventNote, null),
                (TimeLineEventCode.WHOLE_SALE, baseDate.AddDays(11), baseDate.AddDays(11), eventNote, dealer.Code),
            };

        // test
        KitService service = null;

        var payloads = new List<MutationPayload<KitTimelineEvent>>();

        foreach (var entry in timelineEventItems) {
            var input = new KitTimelineEventInput {
                KitNo = kit.KitNo,
                EventType = entry.eventType,
                EventDate = entry.eventDate,
                EventNote = entry.eventNode,
                DealerCode = entry.dealerCode
            };
            service = new KitService(context, entry.trxDate, planBuildLeadTimeDays);
            var payload = await service.CreateKitTimelineEvent(input);
            payloads.Add(payload);
        }

        // assert
        var timelineEvents = context.KitTimelineEvents.ToList();

        Assert.Equal(timelineEventItems.Count, timelineEvents.Count);

        timelineEvents.ForEach(entry => {
            Assert.Equal(eventNote, entry.EventNote);
        });
    }

    [Fact]
    public async Task Create_kit_timline_event_removes_prior_events_of_the_same_type() {
        // setup
        var kit = context.Kits.First();
        await Gen_ShipmentLot_ForKit(kit.KitNo);

        var before_count = context.KitTimelineEvents.Count();

        var originalDate = new DateTime(2020, 11, 28);
        var newDate = new DateTime(2020, 11, 30);

        var dto = new KitTimelineEventInput {
            KitNo = kit.KitNo,
            EventType = TimeLineEventCode.CUSTOM_RECEIVED,
            EventDate = originalDate
        };
        var dto2 = new KitTimelineEventInput {
            KitNo = kit.KitNo,
            EventType = TimeLineEventCode.CUSTOM_RECEIVED,
            EventDate = newDate
        };

        var service = new KitService(context, DateTime.Now, planBuildLeadTimeDays);
        // test
        await service.CreateKitTimelineEvent(dto);
        await service.CreateKitTimelineEvent(dto2);

        var after_count = context.KitTimelineEvents.Count();

        // assert
        Assert.Equal(0, before_count);
        Assert.Equal(2, after_count);

        var originalEntry = context.KitTimelineEvents.FirstOrDefault(t => t.Kit.VIN == kit.VIN && t.RemovedAt != null);
        var latestEntry = context.KitTimelineEvents.FirstOrDefault(t => t.Kit.VIN == kit.VIN && t.RemovedAt == null);

        Assert.Equal(originalEntry.EventDate, originalDate);
        Assert.Equal(newDate, latestEntry.EventDate);
    }

    [Fact]
    public async Task Cannot_add_duplicate_kit_timline_event_if_same_type_and_date_and_note() {
        // setup
        var kit = context.Kits.First();
        await Gen_ShipmentLot_ForKit(kit.KitNo);

        var originalDate = new DateTime(2020, 11, 28);
        var newDate = new DateTime(2020, 11, 30);
        var eventNote = "EN 78889";

        var dto = new KitTimelineEventInput {
            KitNo = kit.KitNo,
            EventType = TimeLineEventCode.CUSTOM_RECEIVED,
            EventDate = originalDate,
            EventNote = eventNote
        };
        var dto2 = new KitTimelineEventInput {
            KitNo = kit.KitNo,
            EventType = TimeLineEventCode.CUSTOM_RECEIVED,
            EventDate = newDate,
            EventNote = eventNote
        };

        // test
        var service = new KitService(context, DateTime.Now, planBuildLeadTimeDays);
        await service.CreateKitTimelineEvent(dto);
        await service.CreateKitTimelineEvent(dto2);
        var payload = await service.CreateKitTimelineEvent(dto2);

        // assert
        var after_count = context.KitTimelineEvents.Count();
        Assert.Equal(2, after_count);
        var errorsMessage = payload.Errors.Select(t => t.Message).First();
        var expectedMessage = "duplicate kit timeline event";

        Assert.Equal(expectedMessage, errorsMessage.Substring(0, expectedMessage.Length));
    }

    [Fact]
    public async Task Can_create_kit_timeline_event_by_lot() {
        // setup
        var lot = context.Lots
            .Include(t => t.Kits)
            .First();
        await Gen_ShipmentLot(lot.LotNo);

        var kitCount = lot.Kits.Count;

        var baseDate = DateTime.Now.Date;
        var eventDate = baseDate.AddDays(-10);
        var eventNote = Util.RandomString(EntityFieldLen.Event_Note);
        var input = new LotTimelineEventInput {
            LotNo = lot.LotNo,
            EventType = TimeLineEventCode.CUSTOM_RECEIVED,
            EventDate = eventDate,
            EventNote = eventNote
        };

        // test
        var service = new KitService(context, baseDate, planBuildLeadTimeDays);
        var payload = await service.CreateLotTimelineEvent(input);

        var errorCount = payload.Errors.Count;
        Assert.Equal(0, errorCount);

        var timelineEvents = context.KitTimelineEvents.Where(t => t.Kit.Lot.LotNo == input.LotNo)
            .Include(t => t.Kit)
            .Include(t => t.EventType).ToList();

        var timelineEventCount = timelineEvents.Count;
        Assert.Equal(kitCount, timelineEventCount);

        foreach (var timelineEvent in timelineEvents) {
            Assert.Equal(eventDate, timelineEvent.EventDate);
            Assert.Equal(eventNote, timelineEvent.EventNote);
        }
    }

    [Fact]
    public async Task Cannot_create_kit_timeline_event_by_lot_with_dupliate_date() {
        // setup
        var lot = context.Lots.First();
        await Gen_ShipmentLot(lot.LotNo);

        var baseDate = DateTime.Now.Date;
        var event_date = baseDate.AddDays(1);
        var event_date_trx = baseDate.AddDays(2);
        var eventNote = Util.RandomString(EntityFieldLen.Event_Note);
        var input = new LotTimelineEventInput {
            LotNo = lot.LotNo,
            EventType = TimeLineEventCode.CUSTOM_RECEIVED,
            EventDate = event_date,
            EventNote = eventNote
        };

        // test
        var service = new KitService(context, event_date_trx, planBuildLeadTimeDays);
        var payload = await service.CreateLotTimelineEvent(input);

        var errorCount = payload.Errors.Count;
        Assert.Equal(0, errorCount);

        var payload_2 = await service.CreateLotTimelineEvent(input);
        var errorCount_2 = payload_2.Errors.Count;
        Assert.Equal(1, errorCount_2);

        var errorMessage = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
        var expectedMessage = "duplicate kit timeline event";
        var actualMessage = errorMessage.Substring(0, expectedMessage.Length);
        Assert.Equal(expectedMessage, actualMessage);
    }

    [Fact]
    public async Task Cannot_create_lot_custom_receive_with_date_6_months_ago() {
        // setup
        var lot = context.Lots.First();
        await Gen_ShipmentLot(lot.LotNo);

        var baseDate = DateTime.Now.Date;
        var event_date = baseDate.AddMonths(-6).AddDays(-1);
        var eventNote = Util.RandomString(EntityFieldLen.Event_Note);
        var input = new LotTimelineEventInput {
            LotNo = lot.LotNo,
            EventType = TimeLineEventCode.CUSTOM_RECEIVED,
            EventDate = event_date,
            EventNote = eventNote
        };

        // test
        var service = new KitService(context, baseDate, planBuildLeadTimeDays);
        var payload = await service.CreateLotTimelineEvent(input);

        var expectedError = "custom received cannot be more than 6 months ago";
        var actualErrorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
        Assert.Equal(expectedError, actualErrorMessage);
    }

    [Fact]
    public async Task Can_change_kit_component_production_station() {
        // setup
        var kit = await context.Kits
            .Include(t => t.KitComponents).ThenInclude(t => t.ProductionStation)
            .FirstOrDefaultAsync();
        var productionStationCodes = await context.ProductionStations.Select(t => t.Code).ToListAsync();
        var kitComponent = kit.KitComponents.FirstOrDefault();
        var newStationCode = productionStationCodes.First(code => code != kitComponent.ProductionStation.Code);

        var service = new KitService(context, DateTime.Now.Date, 6);

        var paylaod = service.ChangeKitComponentProductionStation(new KitComponentProductionStationInput {
            KitComponentId = kitComponent.Id,
            ProductionStationCode = newStationCode
        });

        var kitComponent_2 = await context.KitComponents
            .Include(t => t.ProductionStation)
            .FirstOrDefaultAsync(t => t.Id == kitComponent.Id);

        Assert.Equal(newStationCode, kitComponent_2.ProductionStation.Code);
    }

    [Fact]
    public async Task Kit_model_component_diff_works() {

        // setup
        var service = new VehicleModelService(context);

        var model = await context.VehicleModels
            .Include(t => t.ModelComponents).ThenInclude(t => t.Component)
            .Include(t => t.ModelComponents).ThenInclude(t => t.ProductionStation)
            .FirstAsync();

        var kit = await context.Kits
            .Include(t => t.KitComponents).ThenInclude(t => t.Component)
            .Include(t => t.KitComponents).ThenInclude(t => t.ProductionStation)
            .Where(t => t.Lot.Model.Code == model.Code).FirstOrDefaultAsync();

        var diff = await service.GetKitModelComponentDiff(kit.KitNo);
        Assert.True(0 == diff.InModelButNoKit.Count); // ensure model compoents maatch kit componetns
        Assert.True(0 == diff.InKitButNoModel.Count); // ensure model compoents maatch kit componetns

        var componentToRemove = model.ModelComponents
            .OrderBy(t => t.ProductionStation.Code).ThenBy(t => t.Component.Code)
            .First();
        // remove model component 
        var saveModelInput = new VehicleModelInput {
            Id = model.Id,
            Code = model.Code,
            Description = model.Description,
            ComponentStationInputs = model.ModelComponents
                .Where(t =>
                    !(
                        t.Component.Code == componentToRemove.Component.Code &&
                        t.ProductionStation.Code == componentToRemove.ProductionStation.Code
                    ))
                .Select(t => new ComponentStationInput {
                    ComponentCode = t.Component.Code,
                    ProductionStationCode = t.ProductionStation.Code
                }).ToList()
        };

        await service.Save(saveModelInput);

        diff = await service.GetKitModelComponentDiff(kit.KitNo);
        Assert.True(1 == diff.InKitButNoModel.Count);

        Assert.Equal(componentToRemove.Component.Code, diff.InKitButNoModel[0].ComponentCode);
        Assert.Equal(componentToRemove.Component.Code, diff.InKitButNoModel[0].ComponentCode);
    }

    [Fact]
    public async Task Can_sync_kit_model_components_if_model_component_removed() {
        // setup
        var model = await context.VehicleModels.FirstOrDefaultAsync();
        await RemoveOneComponentFromModel(model);

        var kit = await context.Kits.Where(t => t.Lot.ModelId == model.Id).FirstOrDefaultAsync();

        var service = new VehicleModelService(context);
        var diff = await service.GetKitModelComponentDiff(kit.KitNo);

        Assert.True(1 == diff.InKitButNoModel.Count);

        // test 1: will remove model from kit
        await service.SyncKfitModelComponents(kit.KitNo);

        diff = await service.GetKitModelComponentDiff(kit.KitNo);
        Assert.True(0 == diff.InKitButNoModel.Count);
        Assert.True(0 == diff.InModelButNoKit.Count);
    }

    [Fact]
    public async Task Can_sync_kit_model_components_if_kit_component_removed() {
        // setup
        var kit = await context.Kits
            .Include(t => t.KitComponents)
            .FirstOrDefaultAsync();
        var kitComponet = kit.KitComponents.First();
        kitComponet.RemovedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        var service = new VehicleModelService(context);
        var diff = await service.GetKitModelComponentDiff(kit.KitNo);

        Assert.True(1 == diff.InModelButNoKit.Count);
        Assert.True(0 == diff.InKitButNoModel.Count);

        // test 1: will remove model from kit
        await service.SyncKfitModelComponents(kit.KitNo);

        diff = await service.GetKitModelComponentDiff(kit.KitNo);
        Assert.True(0 == diff.InKitButNoModel.Count);
        Assert.True(0 == diff.InModelButNoKit.Count);
    }

    private async Task RemoveOneComponentFromModel(VehicleModel model) {

        var componentToRemove = model.ModelComponents
            .OrderBy(t => t.ProductionStation.Code).ThenBy(t => t.Component.Code)
            .First();

        var saveModelInput = new VehicleModelInput {
            Id = model.Id,
            Code = model.Code,
            Description = model.Description,
            ComponentStationInputs = model.ModelComponents
                .Where(t =>
                    !(
                        t.Component.Code == componentToRemove.Component.Code &&
                        t.ProductionStation.Code == componentToRemove.ProductionStation.Code
                    ))
                .Select(t => new ComponentStationInput {
                    ComponentCode = t.Component.Code,
                    ProductionStationCode = t.ProductionStation.Code
                }).ToList()
        };
        var service = new VehicleModelService(context);
        await service.Save(saveModelInput);
    }
}

