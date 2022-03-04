#nullable enable


namespace SKD.Service;

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

    public async Task<MutationResult<KitVinImport>> ImportVIN(VinFile input) {
        try {
            MutationResult<KitVinImport> result = new();
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

                // bool kitVinAlreadyExists =   kit.KitVins.Any(t => t.Kit.KitNo == inputKitVin.KitNo && t.VIN == inputKitVin.VIN);
                bool kitVinAlreadyExists = await context.KitVins.AnyAsync(t => t.Kit.KitNo == inputKitVin.KitNo && t.VIN == inputKitVin.VIN);

                if (!kitVinAlreadyExists) {
                    var kit = await context.Kits
                        .Include(t => t.KitVins)
                        .FirstAsync(t => t.KitNo == inputKitVin.KitNo);

                    // set kit.VIN to new VIN & add new kit.KinVin entry
                    kit.VIN = inputKitVin.VIN;
                    kitVinImport.KitVins.Add(new KitVin {
                        Kit = kit,
                        VIN = inputKitVin.VIN
                    });
                    // Flag prior KitVin: RemovedAt
                    kit.KitVins
                        .Where(t => t.VIN != inputKitVin.VIN && t.RemovedAt == null)
                        .ToList().ForEach(kv => {
                            kv.RemovedAt = DateTime.UtcNow;
                        });
                }
            }

            await context.SaveChangesAsync();

            result.Payload = kitVinImport;
            return result;
        } catch (System.Exception ex) {
            Console.WriteLine(ex.Message);
            throw;
        }
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
            errors.Add(new Error("", $"Already imported VIN file  {input.PlantCode}-{input.Sequence}"));
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
    public async Task<MutationResult<KitTimelineEvent>> CreateKitTimelineEvent(KitTimelineEventInput input) {
        MutationResult<KitTimelineEvent> result = new();
        result.Errors = await ValidateCreateKitTimelineEvent(input);
        if (result.Errors.Count > 0) {
            return result;
        }

        var kit = await context.Kits
            .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
            .FirstAsync(t => t.KitNo == input.KitNo);

        // mark other timeline events of the same type as removed for this kit
        kit.TimelineEvents
            .Where(t => t.EventType.Code == input.EventCode)
            .ToList().ForEach(timelieEvent => {
                if (timelieEvent.RemovedAt == null) {
                    timelieEvent.RemovedAt = DateTime.UtcNow;
                }
            });

        // create timeline event and add to kit
        var newTimelineEvent = new KitTimelineEvent {
            EventType = await context.KitTimelineEventTypes.FirstOrDefaultAsync(t => t.Code == input.EventCode),
            EventDate = input.EventDate,
            EventNote = input.EventNote
        };

        // Set kit dealer code if provided
        if (!String.IsNullOrWhiteSpace(input.DealerCode)) {
            kit.Dealer = await context.Dealers.FirstOrDefaultAsync(t => t.Code == input.DealerCode);
        }

        kit.TimelineEvents.Add(newTimelineEvent);

        // save
        result.Payload = newTimelineEvent;
        await context.SaveChangesAsync();
        return result;
    }

    public async Task<List<Error>> ValidateCreateKitTimelineEvent(KitTimelineEventInput input) {
        var errors = new List<Error>();

        // kitNo
        var kit = await context.Kits.AsNoTracking()
            .Include(t => t.Lot)
            .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
            .Include(t => t.Dealer)
            .FirstOrDefaultAsync(t => t.KitNo == input.KitNo);

        var planBuildType = await context.KitTimelineEventTypes.FirstAsync(t => t.Code == TimeLineEventCode.PLAN_BUILD);
        var verifyVinType = await context.KitTimelineEventTypes.FirstAsync(t => t.Code == TimeLineEventCode.VERIFY_VIN);
        var inputEventType = await context.KitTimelineEventTypes.FirstAsync(t => t.Code == input.EventCode);

        if (kit == null) {
            errors.Add(new Error("KitNo", $"kit not found for kitNo: {input.KitNo}"));
            return errors;
        }


        /* Remove feature: 2022-02-11, problem with ship file imports from Ford
        // shipment missing
        var hasAssociatedShipment = await context.ShipmentLots.AnyAsync(t => t.Lot.LotNo == kit.Lot.LotNo);
        if (!hasAssociatedShipment) {
            errors.Add(new Error("", $"shipment missing for lot: {kit.Lot.LotNo}"));
            return errors;
        }
        */

        // duplicate kit timeline event
        var duplicate = kit.TimelineEvents
            .OrderByDescending(t => t.CreatedAt)
            .Where(t => t.RemovedAt == null)
            .Where(t => t.EventType.Code == input.EventCode)
            .Where(t => t.EventDate == input.EventDate)
            .Where(t => t.EventNote == input.EventNote)
            .FirstOrDefault();

        if (duplicate != null) {
            var dateStr = input.EventDate.ToShortDateString();
            errors.Add(new Error("", $"duplicate kit timeline event: {input.EventCode} {dateStr} "));
            return errors;
        }

        // missing prerequisite timeline events
        var currentTimelineEventType = await context.KitTimelineEventTypes
            .FirstAsync(t => t.Code == input.EventCode);

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
        if (input.EventCode == TimeLineEventCode.CUSTOM_RECEIVED) {
            if (currentDate <= input.EventDate) {
                errors.Add(new Error("", $"custom received date must be before current date"));
                return errors;
            }
        }

        // PLAN_BUILD 
        if (input.EventCode == TimeLineEventCode.PLAN_BUILD) {
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
        
        // WHOLESALE kit must be associated with dealer to proceed
        if (input.EventCode == TimeLineEventCode.WHOLE_SALE) {
            if (String.IsNullOrWhiteSpace(input.DealerCode)) {
                if (kit.Dealer == null) {
                    errors.Add(new Error("", $"Kit must be associated with dealer ${kit.KitNo}"));
                    return errors;
                }
            } else {
                var dealer = await context.Dealers.FirstOrDefaultAsync(t => t.Code == input.DealerCode);
                if (dealer == null) {
                    errors.Add(new Error("", $"Dealer not found for code ${input.DealerCode}"));
                    return errors;
                }
            }
        }

        // VIN Required for events sequence after PLAN BUILD
        if (inputEventType.Sequence > planBuildType.Sequence && String.IsNullOrWhiteSpace(kit.VIN)) {
            errors.Add(new Error("", $"Kit does not have VIN, cannot save {input.EventCode} event"));
            return errors;
        }

        // Event date cannot be in the future for events after VERIFY VIN
        if (inputEventType.Sequence > verifyVinType.Sequence) {
            if (input.EventDate.Date > currentDate.Date) {
                errors.Add(new Error("", $"Date cannot be in the future"));
                return errors;
            }
        }

        return errors;
    }

    #endregion

    #region create lot timeline event
    public async Task<MutationResult<Lot>> CreateLotTimelineEvent(LotTimelineEventInput input) {
        MutationResult<Lot> result = new();
        result.Errors = await ValidateCreateLotTimelineEvent(input);
        if (result.Errors.Count > 0) {
            return result;
        }

        var kitLot = await context.Lots
            .Include(t => t.Kits)
                .ThenInclude(t => t.TimelineEvents)
                .ThenInclude(t => t.EventType)
            .FirstAsync(t => t.LotNo == input.LotNo);

        foreach (var kit in kitLot.Kits) {

            // mark other timeline events of the same type as removed for this kit
            kit.TimelineEvents
                .Where(t => t.EventType.Code == input.EventCode)
                .ToList().ForEach(timelieEvent => {
                    if (timelieEvent.RemovedAt == null) {
                        timelieEvent.RemovedAt = DateTime.UtcNow;
                    }
                });

            // create timeline event and add to kit
            var newTimelineEvent = new KitTimelineEvent {
                EventType = await context.KitTimelineEventTypes.FirstOrDefaultAsync(t => t.Code == input.EventCode),
                EventDate = input.EventDate,
                EventNote = input.EventNote
            };

            kit.TimelineEvents.Add(newTimelineEvent);

        }

        // // save
        result.Payload = kitLot;
        await context.SaveChangesAsync();
        return result;
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
        /* Remove feature: 2022-02-11, problem with ship file imports from Ford
        // shipment missing        
        var hasAssociatedShipment = await context.ShipmentLots.AnyAsync(t => t.Lot.LotNo == lot.LotNo);
        if (!hasAssociatedShipment) {
            errors.Add(new Error("", $"shipment missing for lot: {lot.LotNo}"));
            return errors;
        }
        */

        // duplicate 
        var duplicateTimelineEventsFound = lot.Kits.SelectMany(t => t.TimelineEvents)
            .OrderByDescending(t => t.CreatedAt)
            .Where(t => t.RemovedAt == null)
            .Where(t => t.EventType.Code == input.EventCode)
            .Where(t => t.EventDate == input.EventDate)
            .ToList();

        if (duplicateTimelineEventsFound.Count > 0) {
            var dateStr = input.EventDate.ToShortDateString();
            errors.Add(new Error("", $"duplicate kit timeline event: {input.LotNo}, Type: {input.EventCode} Date: {dateStr} "));
            return errors;
        }

        // snapshot already taken
        if (await SnapshotAlreadyTaken(input)) {
            errors.Add(new Error("", $"cannot update {input.EventCode} after snapshot taken"));
            return errors;
        }

        // CUSTOM_RECEIVED 
        if (input.EventCode == TimeLineEventCode.CUSTOM_RECEIVED) {
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


    public async Task<MutationResult<KitComponent>> ChangeKitComponentProductionStation(KitComponentProductionStationInput input) {
        MutationResult<KitComponent> result = new();
        result.Errors = await ValidateChangeKitComponentStationImput(input);
        if (result.Errors.Count > 0) {
            return result;
        }

        var kitComponent = await context.KitComponents.FirstAsync(t => t.Id == input.KitComponentId);
        var productionStation = await context.ProductionStations.FirstAsync(t => t.Code == input.ProductionStationCode);

        kitComponent.ProductionStation = productionStation;
        // // save
        await context.SaveChangesAsync();
        result.Payload = kitComponent;
        return result;
    }

    public async Task<List<Error>> ValidateChangeKitComponentStationImput(KitComponentProductionStationInput input) {
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

        return input.EventCode switch {
            TimeLineEventCode.CUSTOM_RECEIVED => kitSnapshot.CustomReceived != null,
            TimeLineEventCode.PLAN_BUILD => kitSnapshot.PlanBuild != null,
            TimeLineEventCode.BUILD_COMPLETED => kitSnapshot.BuildCompleted != null,
            TimeLineEventCode.GATE_RELEASED => kitSnapshot.GateRelease != null,
            TimeLineEventCode.WHOLE_SALE => kitSnapshot.Wholesale != null,
            _ => false
        };
    }
}
