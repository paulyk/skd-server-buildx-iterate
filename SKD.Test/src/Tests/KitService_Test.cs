using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class KitServiceTest : TestBase {

        int planBuildLeadTimeDays = 2;
        public KitServiceTest() {
            context = GetAppDbContext();
            Gen_Baseline_Test_Seed_Data();
        }

        [Fact]
        public async Task can_import_kit_vin() {
            // setup
            var lot = context.Lots.First();
            var plant = context.Plants.First();
            var partnerPlantCode = Gen_PartnerPLantCode();
            var sequence = 3;

            var input = new ImportVinInput {
                PlantCode = plant.Code,
                PartnerPlantCode = partnerPlantCode,
                Sequence = sequence,
                Kits = lot.Kits.Select(t => new ImportVinInput.KitVin {
                    LotNo = lot.LotNo,
                    KitNo = t.KitNo,
                    VIN = Gen_VIN()
                }).ToList()
            };

            // test
            var kits = await context.Kits.Where(t => t.Lot.LotNo == lot.LotNo).ToListAsync();
            var lot_vehicles_count = kits.Count();
            var with_vin_count = kits.Count(t => t.VIN != "");
            Assert.True(0 == with_vin_count);

            var service = new KitService(context, DateTime.Now, planBuildLeadTimeDays);
            var payload = await service.ImportVIN(input);

            // assert
            var kitVinImport = await context.KitVinImports.FirstOrDefaultAsync(t => t.Plant.Code == input.PlantCode && t.Sequence == input.Sequence);
            Assert.NotNull(kitVinImport);

            var kitvin_count = await context.KitVins.CountAsync(t => t.KitVinImportId == kitVinImport.Id);
            var expected_count = input.Kits.Count();
            Assert.True(expected_count == kitvin_count);

            // partner code
            Assert.Equal(input.PartnerPlantCode, kitVinImport.PartnerPlantCode);

            kits = await context.Kits.Where(t => t.Lot.LotNo == lot.LotNo).ToListAsync();
            with_vin_count = kits.Count(t => t.VIN != "");
            Assert.True(lot_vehicles_count == with_vin_count);
        }

        [Fact]
        public async Task cannot_import_kit_vins_if_kits_not_found() {
            // setup
            var lot = context.Lots.First();
            var plant = context.Plants.First();
            var partnerPlantCode = Gen_PartnerPLantCode();
            var sequence = 3;

            var kitVinDto = new ImportVinInput {
                PlantCode = plant.Code,
                PartnerPlantCode = partnerPlantCode,
                Sequence = sequence,
                Kits = lot.Kits.Select(t => new ImportVinInput.KitVin {
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
        public async Task cannot_import_kit_vins_with_kits_in_payload() {
            // setup
            var lot = context.Lots.First();
            var plant = context.Plants.First();
            var partnerPlantCode = Gen_PartnerPLantCode();
            var sequence = 3;

            var lotVehicles = lot.Kits.ToList();

            var input = new ImportVinInput {
                PlantCode = plant.Code,
                PartnerPlantCode = partnerPlantCode,
                Sequence = sequence,
            };

            input.Kits = new List<ImportVinInput.KitVin>() {
                new ImportVinInput.KitVin {LotNo = lot.LotNo, KitNo = lotVehicles[0].KitNo, VIN = Gen_VIN() },
                new ImportVinInput.KitVin {LotNo = lot.LotNo, KitNo = lotVehicles[1].KitNo, VIN = Gen_VIN() },
                new ImportVinInput.KitVin {LotNo = lot.LotNo, KitNo = lotVehicles[2].KitNo, VIN = Gen_VIN() },
                new ImportVinInput.KitVin {LotNo = lot.LotNo, KitNo = lotVehicles[3].KitNo, VIN = Gen_VIN() },
                // duplicate kits
                new ImportVinInput.KitVin {LotNo = lot.LotNo, KitNo = lotVehicles[5].KitNo, VIN = Gen_VIN() },
                new ImportVinInput.KitVin {LotNo = lot.LotNo, KitNo = lotVehicles[5].KitNo, VIN = Gen_VIN() },
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
        public async Task can_create_kit_timeline_events() {
            // setup        
            var timelineEvents = new List<(string eventTypeCode, DateTime eventDate)>() {
                (TimeLineEventType.CUSTOM_RECEIVED.ToString(), new DateTime(2020, 11, 1)),
                (TimeLineEventType.PLAN_BUILD.ToString(), new DateTime(2020, 11, 8)),
                (TimeLineEventType.BULD_COMPLETED.ToString(), new DateTime(2020, 11, 22)),
                (TimeLineEventType.GATE_RELEASED.ToString(), new DateTime(2020, 11, 26)),
                (TimeLineEventType.WHOLE_SALE.ToString(), new DateTime(2020, 11, 30)),
            };

            // test
            var kit = context.Kits.First();

            var service = new KitService(context, DateTime.Now, planBuildLeadTimeDays);
            var payloads = new List<MutationPayload<KitTimelineEvent>>();

            var before_count = context.KitTimelineEvents.Count();


            foreach (var entry in timelineEvents) {
                var dto = new KitTimelineEventInput {
                    KitNo = kit.KitNo,
                    EventType = Enum.Parse<TimeLineEventType>(entry.eventTypeCode),
                    EventDate = entry.eventDate,
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
        public async Task error_if_custom_receive_date_greater_than_current_date() {

            // test
            var kit = context.Kits.First();

            var currentDate = DateTime.Now.Date;
            var service = new KitService(context, currentDate, planBuildLeadTimeDays);

            var input_1 = new KitTimelineEventInput {
                KitNo = kit.KitNo,
                EventType = TimeLineEventType.CUSTOM_RECEIVED,
                EventDate = currentDate
            };

            var input_2 = new KitTimelineEventInput {
                KitNo = kit.KitNo,
                EventType = TimeLineEventType.CUSTOM_RECEIVED,
                EventDate = currentDate.AddDays(-1)
            };

            // test
            var payload_1 = await service.CreateKitTimelineEvent(input_1);
            var payload_2 = await service.CreateKitTimelineEvent(input_2);

            // assert
            var expectedError = "custom received date must be before current date";
            var actualMessage = payload_1.Errors.Select(t => t.Message).FirstOrDefault();
            Assert.Equal(expectedError, actualMessage);

            var errorCount = payload_2.Errors.Count();
            Assert.Equal(0, errorCount);
        }

        [Fact]
        public async Task cannot_create_kit_timeline_events_out_of_sequence() {
            // setup
            var baseDate = DateTime.Now.Date;
            var timelineEvents = new List<(string eventTypeCode, DateTime trxDate, DateTime eventDate)>() {
                (TimeLineEventType.CUSTOM_RECEIVED.ToString(), baseDate.AddDays(1),  baseDate.AddDays(6)),
                (TimeLineEventType.BULD_COMPLETED.ToString(), baseDate.AddDays(2), baseDate.AddDays(2)),
            };

            // test
            var kit = context.Kits.First();
            KitService service = null;
            var payloads = new List<MutationPayload<KitTimelineEvent>>();

            foreach (var entry in timelineEvents) {
                var input = new KitTimelineEventInput {
                    KitNo = kit.KitNo,
                    EventType = Enum.Parse<TimeLineEventType>(entry.eventTypeCode),
                    EventDate = entry.eventDate,
                };
                service = new KitService(context, entry.trxDate, planBuildLeadTimeDays);
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
        public async Task create_kit_timeline_event_with_note() {
            // setup
            var kit = context.Kits.First();
            var eventNote = "DLR_9977";

            var baseDate = DateTime.Now.Date;
            var timelineEventItems = new List<(string eventTypeCode, DateTime trxDate, DateTime eventDate, string eventNode)>() {
                (TimeLineEventType.CUSTOM_RECEIVED.ToString(), baseDate.AddDays(2), baseDate.AddDays(1) , eventNote),
                (TimeLineEventType.PLAN_BUILD.ToString(), baseDate.AddDays(3), baseDate.AddDays(5), eventNote),
                (TimeLineEventType.BULD_COMPLETED.ToString(), baseDate.AddDays(8), baseDate.AddDays(8), eventNote),
                (TimeLineEventType.GATE_RELEASED.ToString(), baseDate.AddDays(10), baseDate.AddDays(10), eventNote),
                (TimeLineEventType.WHOLE_SALE.ToString(), baseDate.AddDays(11), baseDate.AddDays(11), eventNote),
            };

            // test
            KitService service = null;

            var payloads = new List<MutationPayload<KitTimelineEvent>>();

            foreach (var entry in timelineEventItems) {
                var input = new KitTimelineEventInput {
                    KitNo = kit.KitNo,
                    EventType = Enum.Parse<TimeLineEventType>(entry.eventTypeCode),
                    EventDate = entry.eventDate,
                    EventNote = entry.eventNode
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
        public async Task create_kit_timline_event_removes_prior_events_of_the_same_type() {
            // setup
            var kit = context.Kits.First();
            var before_count = context.KitTimelineEvents.Count();

            var originalDate = new DateTime(2020, 11, 28);
            var newDate = new DateTime(2020, 11, 30);

            var dto = new KitTimelineEventInput {
                KitNo = kit.KitNo,
                EventType = TimeLineEventType.CUSTOM_RECEIVED,
                EventDate = originalDate
            };
            var dto2 = new KitTimelineEventInput {
                KitNo = kit.KitNo,
                EventType = TimeLineEventType.CUSTOM_RECEIVED,
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
        public async Task cannot_add_duplicate_kit_timline_event_if_same_type_and_date_and_note() {
            // setup
            var kit = context.Kits.First();

            var originalDate = new DateTime(2020, 11, 28);
            var newDate = new DateTime(2020, 11, 30);
            var eventNote = "EN 78889";

            var dto = new KitTimelineEventInput {
                KitNo = kit.KitNo,
                EventType = TimeLineEventType.CUSTOM_RECEIVED,
                EventDate = originalDate,
                EventNote = eventNote
            };
            var dto2 = new KitTimelineEventInput {
                KitNo = kit.KitNo,
                EventType = TimeLineEventType.CUSTOM_RECEIVED,
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
        public async Task can_create_kit_timeline_event_by_lot() {
            // setup
            var vehicleLot = context.Lots
                .Include(t => t.Kits)
                .First();
            var vehicleCoount = vehicleLot.Kits.Count();

            var eventDate = new DateTime(2020, 11, 30);
            var eventNote = Util.RandomString(EntityFieldLen.Event_Note);
            var dto = new LotTimelineEventInput {
                LotNo = vehicleLot.LotNo,
                EventType = TimeLineEventType.CUSTOM_RECEIVED,
                EventDate = eventDate,
                EventNote = eventNote
            };

            // test
            var service = new KitService(context, DateTime.Now, planBuildLeadTimeDays);
            var payload = await service.CreateLotTimelineEvent(dto);

            var errorCount = payload.Errors.Count;
            Assert.Equal(0, errorCount);

            var timelineEvents = context.KitTimelineEvents.Where(t => t.Kit.Lot.LotNo == dto.LotNo)
                .Include(t => t.Kit)
                .Include(t => t.EventType).ToList();

            var timelineEventCount = timelineEvents.Count();
            Assert.Equal(vehicleCoount, timelineEventCount);

            foreach (var timelineEvent in timelineEvents) {
                Assert.Equal(eventDate, timelineEvent.EventDate);
                Assert.Equal(eventNote, timelineEvent.EventNote);
            }
        }

        [Fact]
        public async Task cannot_create_kit_timeline_event_by_lot_with_dupliate_date() {
            // setup
            var lot = context.Lots.First();

            var baseDate = DateTime.Now.Date;
            var event_date = baseDate.AddDays(1);
            var event_date_trx = baseDate.AddDays(2);
            var eventNote = Util.RandomString(EntityFieldLen.Event_Note);
            var input = new LotTimelineEventInput {
                LotNo = lot.LotNo,
                EventType = TimeLineEventType.CUSTOM_RECEIVED,
                EventDate = event_date,
                EventNote = eventNote
            };

            // test
            var service = new KitService(context, event_date_trx, planBuildLeadTimeDays);
            var payload = await service.CreateLotTimelineEvent(input);

            var errorCount = payload.Errors.Count;
            Assert.Equal(0, errorCount);

            var payload_2 = await service.CreateLotTimelineEvent(input);
            var errorCount_2 = payload_2.Errors.Count();
            Assert.Equal(1, errorCount_2);

            var errorMessage = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
            var expectedMessage = "duplicate kit timeline event";
            var actualMessage = errorMessage.Substring(0, expectedMessage.Length);
            Assert.Equal(expectedMessage, actualMessage);
        }

        [Fact]
        public async Task can_change_kit_component_production_station() {
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
        public async Task kit_model_component_diff_works() {

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
                Name = model.Name,
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

            await service.SaveVehicleModel(saveModelInput);

            diff = await service.GetKitModelComponentDiff(kit.KitNo);
            Assert.True(1 == diff.InKitButNoModel.Count);

            Assert.Equal(componentToRemove.Component.Code, diff.InKitButNoModel[0].ComponentCode);
            Assert.Equal(componentToRemove.Component.Code, diff.InKitButNoModel[0].ComponentCode);
        }


        [Fact]
        public async Task can_sync_kit_model_components_if_model_component_removed() {
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
        public async Task can_sync_kit_model_components_if_kit_component_removed() {
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
                Name = model.Name,
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
            await service.SaveVehicleModel(saveModelInput);
        }
    }


}
