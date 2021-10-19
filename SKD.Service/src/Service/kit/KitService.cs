#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SKD.Common;
using SKD.Model;

namespace SKD.Service {

    public class KitService {

        private readonly SkdContext context;
        private readonly DateTime currentDate;
        public readonly int planBuildLeadTimeDays = 6;

        public KitService(SkdContext ctx, DateTime currentDate, int planBuildLeadTimeDays) {
            this.context = ctx;
            this.currentDate = currentDate;
            this.planBuildLeadTimeDays = planBuildLeadTimeDays;
        }

        #region import vin

        public async Task<MutationPayload<KitVinImport>> ImportVIN(VinFile input) {
            MutationPayload<KitVinImport> result = new();
            result.Errors = await ValidateImportVINInput(input);
            if (result.Errors.Any()) {
                return result;
            }

            // new KitVinImport / existing        
            var kitVinImport = new KitVinImport {
                Plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode),
                Sequence = input.Sequence,
                PartnerPlantCode = input.PartnerPlantCode,
            };
            context.KitVinImports.Add(kitVinImport);

            foreach (var inputKitVin in input.Kits) {
                var kit = await context.Kits
                    .Include(t => t.KitVins).ThenInclude(t => t.Kit).ThenInclude(t => t.Lot)
                    .FirstOrDefaultAsync(t => t.KitNo == inputKitVin.KitNo);

                bool kitVinAlreadyExists = kit.KitVins.Any(t => t.Kit.KitNo == inputKitVin.KitNo && t.VIN == inputKitVin.VIN);

                if (!kitVinAlreadyExists) {
                    kit.VIN = inputKitVin.VIN;
                    var kitVin = new KitVin {
                        Kit = kit,
                        VIN = inputKitVin.VIN
                    };
                    kitVinImport.KitVins.Add(kitVin);
                } }

            await context.SaveChangesAsync();
            
            result.Payload = kitVinImport;
            return result;
        }

        public async Task<List<Error>> ValidateImportVINInput(VinFile input) {
            var errors = new List<Error>();

            // plant
            var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode);
            if (plant == null) {
                errors.Add(new Error("", $"Plant code not found {input.PlantCode}"));
                return errors;
            }

            // sequence 
            if (input.Sequence == 0) {
                errors.Add(new Error("", $"Sequence number required"));
                return errors;
            }

            // already imported 
            var kitVinAlreadyImported = await context.KitVinImports.AnyAsync(
                t => t.Plant.Code == input.PlantCode && t.Sequence == input.Sequence
            );

            if (kitVinAlreadyImported) {
                errors.Add(new Error("", $"Already imported plant - sequence {input.PlantCode} - {input.Sequence}"));
                return errors;
            }

            // partner code
            if (String.IsNullOrEmpty(input.PartnerPlantCode)) {
                errors.Add(new Error("", $"Parnter plant code required"));
                return errors;
            } else if (input.PartnerPlantCode.Length != EntityFieldLen.PartnerPlant_Code) {
                errors.Add(new Error("", $"Parnter plant code not valid {input.PartnerPlantCode}"));
                return errors;
            }

            // kits not found
            var kitNos = input.Kits.Select(t => t.KitNo).ToList();
            var existingKitNos = await context.Kits.Where(t => kitNos.Any(kitNo => kitNo == t.KitNo))
                .Select(t => t.KitNo)
                .ToListAsync();

            var kitsNotFound = kitNos.Except(existingKitNos).ToList();
            if (kitsNotFound.Any()) {
                var kitNumbers = String.Join(", ", kitsNotFound);
                errors.Add(new Error("", $"kit numbers not found : {kitNumbers}"));
                return errors;
            }

            // invalid VIN(s)
            var invalidVins = input.Kits
                .Select(t => t.VIN)
                .Where(vin => !Validator.Valid_KitNo(vin))
                .ToList();

            if (invalidVins.Any()) {
                errors.Add(new Error("", $"invalid VINs {String.Join(", ", invalidVins)}"));
                return errors;
            }

            // kits
            var kits = await context.Kits
                .Include(t => t.Lot)
                .Where(t => kitNos.Any(kitNo => kitNo == t.KitNo))
                .ToListAsync();

            // Wehicles with matching kit numbers not found
            var kit_numbers_not_found = new List<string>();
            foreach (var kit in input.Kits) {
                var exists = await context.Kits.AsNoTracking().AnyAsync(t => t.KitNo == kit.KitNo);
                if (!exists) {
                    kit_numbers_not_found.Add(kit.KitNo);
                }
            }
            if (kit_numbers_not_found.Any()) {
                errors.Add(new Error("", $"kit numbers not found {String.Join(", ", kit_numbers_not_found)}"));
                return errors;
            }

            // duplicate kitNo in payload
            var duplicateKitNos = input.Kits
                .GroupBy(t => t.KitNo)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.ToList())
                .Select(t => t.KitNo)
                .Distinct().ToList();

            if (duplicateKitNos.Any()) {
                errors.Add(new Error("lotNo", $"duplicate kitNo(s) in payload: {String.Join(", ", duplicateKitNos)}"));
                return errors;
            }

            return errors;
        }


        #endregion

        #region create kit timeline event
        public async Task<MutationPayload<KitTimelineEvent>> CreateKitTimelineEvent(KitTimelineEventInput input) {
            MutationPayload<KitTimelineEvent> payload = new();
            payload.Errors = await ValidateCreateKitTimelineEvent(input);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            var kit = await context.Kits
                .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .FirstOrDefaultAsync(t => t.KitNo == input.KitNo);

            // mark other timeline events of the same type as removed for this kit
            kit.TimelineEvents
                .Where(t => t.EventType.Code == input.EventType)
                .ToList().ForEach(timelieEvent => {
                    if (timelieEvent.RemovedAt == null) {
                        timelieEvent.RemovedAt = DateTime.UtcNow;
                    }
                });

            // create timeline event and add to kit
            var newTimelineEvent = new KitTimelineEvent {
                EventType = await context.KitTimelineEventTypes.FirstOrDefaultAsync(t => t.Code == input.EventType),
                EventDate = input.EventDate,
                EventNote = input.EventNote
            };

            // if wholesale event set the kit dealer code
            if (input.EventType == TimeLineEventCode.WHOLE_SALE) {
                // As of this moment: There should only be on dealer code.
                kit.Dealer = await context.Dealers.FirstOrDefaultAsync();
            }

            kit.TimelineEvents.Add(newTimelineEvent);

            // save
            payload.Payload = newTimelineEvent;
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<List<Error>> ValidateCreateKitTimelineEvent(KitTimelineEventInput input) {
            var errors = new List<Error>();

            // kitNo
            var kit = await context.Kits.AsNoTracking()
                .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .FirstOrDefaultAsync(t => t.KitNo == input.KitNo);

            if (kit == null) {
                errors.Add(new Error("KitNo", $"kit not found for kitNo: {input.KitNo}"));
                return errors;
            }

            // duplicate kit timeline event
            var duplicate = kit.TimelineEvents
                .OrderByDescending(t => t.CreatedAt)
                .Where(t => t.RemovedAt == null)
                .Where(t => t.EventType.Code == input.EventType)
                .Where(t => t.EventDate == input.EventDate)
                .Where(t => t.EventNote == input.EventNote)
                .FirstOrDefault();

            if (duplicate != null) {
                var dateStr = input.EventDate.ToShortDateString();
                errors.Add(new Error("", $"duplicate kit timeline event: {input.EventType} {dateStr} "));
                return errors;
            }

            // kit timeline event snapshot aready taken
            var exitingKitSnapnshot = await context.KitSnapshots
                .Where(t => t.Kit.KitNo == input.KitNo)
                .Where(t => t.KitTimeLineEventType.Code == input.EventType)
                .FirstOrDefaultAsync();

            if (exitingKitSnapnshot != null) {
                errors.Add(new Error("", $"cannot change date after snapshot taken"));
                return errors;
            }

            // missing prerequisite timeline events
            var currentTimelineEventType = await context.KitTimelineEventTypes
                .FirstOrDefaultAsync(t => t.Code == input.EventType);

            var missingTimlineSequences = Enumerable.Range(1, currentTimelineEventType.Sequence - 1)
                .Where(seq => !kit.TimelineEvents
                .Any(t => t.EventType.Sequence == seq)).ToList();


            if (missingTimlineSequences.Count > 0) {
                var mssingTimelineEventCodes = await context.KitTimelineEventTypes
                    .Where(t => missingTimlineSequences.Any(missingSeq => t.Sequence == missingSeq))
                    .Select(t => t.Code).ToListAsync();

                var text = mssingTimelineEventCodes.Select(t => t.ToString()).Aggregate((a, b) => a + ", " + b);
                errors.Add(new Error("", $"prior timeline event(s) missing {text}"));
                return errors;
            }

            // CUSTOM_RECEIVED 
            if (input.EventType == TimeLineEventCode.CUSTOM_RECEIVED) {
                if (currentDate <= input.EventDate) {
                    errors.Add(new Error("", $"custom received date must be before current date"));
                    return errors;
                }
            }

            // PLAN_BUILD 
            if (input.EventType == TimeLineEventCode.PLAN_BUILD) {
                var custom_receive_date = kit.TimelineEvents
                    .Where(t => t.RemovedAt == null)
                    .Where(t => t.EventType.Code == TimeLineEventCode.CUSTOM_RECEIVED)
                    .Select(t => t.EventDate).First();

                var custom_receive_plus_lead_time_date = custom_receive_date.AddDays(planBuildLeadTimeDays);

                var plan_build_date = input.EventDate;
                if (custom_receive_plus_lead_time_date > plan_build_date) {
                    errors.Add(new Error("", $"plan build must greater custom receive by {planBuildLeadTimeDays} days"));
                    return errors;
                }
            }

            if (input.EventType == TimeLineEventCode.WHOLE_SALE) {
                if (String.IsNullOrWhiteSpace(input.DealerCode)) {
                    errors.Add(new Error("", "Dealer code required"));
                    return errors;
                }
                var dealer = await context.Dealers.FirstOrDefaultAsync(t => t.Code == input.DealerCode);
                if (dealer == null) {
                    errors.Add(new Error("", $"Dealer not found for: {input.DealerCode}"));
                    return errors;
                }
            }

            return errors;
        }

        #endregion

        #region create lot timeline event
        public async Task<MutationPayload<Lot>> CreateLotTimelineEvent(LotTimelineEventInput dto) {
            MutationPayload<Lot> payload = new();
            payload.Errors = await ValidateCreateLotTimelineEvent(dto);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            var kitLot = await context.Lots
                .Include(t => t.Kits)
                    .ThenInclude(t => t.TimelineEvents)
                    .ThenInclude(t => t.EventType)
                .FirstOrDefaultAsync(t => t.LotNo == dto.LotNo);

            foreach (var kit in kitLot.Kits) {

                // mark other timeline events of the same type as removed for this kit
                kit.TimelineEvents
                    .Where(t => t.EventType.Code == dto.EventType)
                    .ToList().ForEach(timelieEvent => {
                        if (timelieEvent.RemovedAt == null) {
                            timelieEvent.RemovedAt = DateTime.UtcNow;
                        }
                    });

                // create timeline event and add to kit
                var newTimelineEvent = new KitTimelineEvent {
                    EventType = await context.KitTimelineEventTypes.FirstOrDefaultAsync(t => t.Code == dto.EventType),
                    EventDate = dto.EventDate,
                    EventNote = dto.EventNote
                };

                kit.TimelineEvents.Add(newTimelineEvent);

            }

            // // save
            payload.Payload = kitLot;
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<List<Error>> ValidateCreateLotTimelineEvent(LotTimelineEventInput input) {
            var errors = new List<Error>();

            var lot = await context.Lots.AsNoTracking()
                .Include(t => t.Kits).ThenInclude(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .FirstOrDefaultAsync(t => t.LotNo == input.LotNo);

            // kit lot 
            if (lot == null) {
                errors.Add(new Error("VIN", $"lot not found for lotNo: {input.LotNo}"));
                return errors;
            }


            // duplicate 
            var duplicateTimelineEventsFound = lot.Kits.SelectMany(t => t.TimelineEvents)
                .OrderByDescending(t => t.CreatedAt)
                .Where(t => t.RemovedAt == null)
                .Where(t => t.EventType.Code == input.EventType)
                .Where(t => t.EventDate == input.EventDate)
                .ToList();

            if (duplicateTimelineEventsFound.Count > 0) {
                var dateStr = input.EventDate.ToShortDateString();
                errors.Add(new Error("", $"duplicate kit timeline event: {input.LotNo}, Type: {input.EventType} Date: {dateStr} "));
                return errors;
            }

            // snapshot already taken
            if (await SnapshotAlreadyTaken(input)) {
                errors.Add(new Error("", $"cannot update {input.EventType} after snapshot taken"));
                return errors;
            }

            // CUSTOM_RECEIVED 
            if (input.EventType == TimeLineEventCode.CUSTOM_RECEIVED) {
                if (input.EventDate.Date >= currentDate) {
                    errors.Add(new Error("", $"custom received date must be before current date"));
                    return errors;
                }
                if (input.EventDate.Date < currentDate.AddMonths(-6)) {
                    errors.Add(new Error("", $"custom received cannot be more than 6 months ago"));
                    return errors;
                }
            }

            return errors;
        }

        #endregion


        public async Task<MutationPayload<KitComponent>> ChangeKitComponentProductionStation(KitComponentProductionStationInput input) {
            MutationPayload<KitComponent> payload = new();
            payload.Errors = await ValidateChangeKitCXomponentStationImput(input);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            var kitComponent = await context.KitComponents.FirstOrDefaultAsync(t => t.Id == input.KitComponentId);
            var productionStation = await context.ProductionStations.FirstOrDefaultAsync(t => t.Code == input.ProductionStationCode);

            kitComponent.ProductionStation = productionStation;
            // // save
            await context.SaveChangesAsync();
            payload.Payload = kitComponent;
            return payload;
        }

        public async Task<List<Error>> ValidateChangeKitCXomponentStationImput(KitComponentProductionStationInput input) {
            var errors = new List<Error>();

            var kitComponent = await context.KitComponents.FirstOrDefaultAsync(t => t.Id == input.KitComponentId);
            if (kitComponent == null) {
                errors.Add(new Error("", $"kit component not found for ${input.KitComponentId}"));
                return errors;
            }

            var productionStation = await context.ProductionStations.FirstOrDefaultAsync(t => t.Code == input.ProductionStationCode);
            if (productionStation == null) {
                errors.Add(new Error("", $"production station not found ${input.ProductionStationCode}"));
                return errors;
            }

            if (kitComponent.ProductionStationId == productionStation.Id) {
                errors.Add(new Error("", $"production station is already set to {input.ProductionStationCode}"));
                return errors;
            }

            return errors;
        }
        public async Task<Boolean> SnapshotAlreadyTaken(LotTimelineEventInput input) {

            var kitSnapshot = await context.KitSnapshots
                .OrderByDescending(t => t.CreatedAt)
                .Where(t => t.Kit.Lot.LotNo == input.LotNo)
                .FirstOrDefaultAsync();

            if (kitSnapshot == null) {
                return false;
            }

            return input.EventType switch {
                TimeLineEventCode.CUSTOM_RECEIVED => kitSnapshot.CustomReceived != null,
                TimeLineEventCode.PLAN_BUILD => kitSnapshot.PlanBuild != null,
                TimeLineEventCode.BUILD_COMPLETED => kitSnapshot.BuildCompleted != null,
                TimeLineEventCode.GATE_RELEASED => kitSnapshot.GateRelease != null,
                TimeLineEventCode.WHOLE_SALE => kitSnapshot.Wholesale != null,
                _ => false
            };
        }
    }
}