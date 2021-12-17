#nullable enable

namespace SKD.Service;

public class ComponentSerialService {
    private readonly SkdContext context;


    public ComponentSerialService(SkdContext ctx) {
        this.context = ctx;
    }

    public async Task<MutationPayload<ComponentSerialDTO>> SaveComponentSerial(ComponentSerialInput input) {
        input = SwapSerial(input);

        MutationPayload<ComponentSerialDTO> payload = new();

        payload.Errors = await ValidateSaveComponentSerial<ComponentSerialInput>(input with { });
        if (payload.Errors.Any()) {
            return payload;
        }

        var kitComponent = await context.KitComponents
            .Include(t => t.Component)
            .FirstAsync(t => t.Id == input.KitComponentId);

        // Some serial1 must be formatted acording to DCWS rules
        // The following formats EN / TR serials if they need adjusting
        // Other compoent serials are returned unchanged.
        var formatSerialResult = DcwsSerialFormatter.FormatSerialIfNeeded(kitComponent.Component.Code, new Serials(input.Serial1, input.Serial2));

        // create
        var componentSerial = new ComponentSerial {
            KitComponentId = input.KitComponentId,
            Serial1 = formatSerialResult.Serials.Serial1,
            Serial2 = formatSerialResult.Serials.Serial2,

            // if inputSerial1 different from formatted then set original serial 1 & 2
            Original_Serial1 = formatSerialResult.Serials.Serial1 != input.Serial1
                ? input.Serial1
                : "",
            Original_Serial2 = formatSerialResult.Serials.Serial2 != input.Serial2
                ? input.Serial2
                : "",
        };

        // Deactivate existing scan if Replace == true
        if (input.Replace) {
            var existintScans = await context.ComponentSerials
                .Where(t => t.KitComponentId == input.KitComponentId && t.RemovedAt == null).ToListAsync();
            existintScans.ForEach(t => t.RemovedAt = DateTime.UtcNow);
        }

        // add             
        context.ComponentSerials.Add(componentSerial);

        // save
        await context.SaveChangesAsync();

        payload.Payload = await context.ComponentSerials
            .Where(t => t.Id == componentSerial.Id)
            .Select(t => new ComponentSerialDTO {
                ComponentSerialId = t.Id,
                VIN = t.KitComponent.Kit.VIN,
                LotNo = t.KitComponent.Kit.Lot.LotNo,
                ComponentCode = t.KitComponent.Component.Code,
                ComponentName = t.KitComponent.Component.Name,
                ProductionStationCode = t.KitComponent.ProductionStation.Code,
                ProductionStationName = t.KitComponent.ProductionStation.Name,
                Serial1 = t.Serial1,
                Serial2 = t.Serial2,
                VerifiedAt = t.VerifiedAt,
                CreatedAt = t.CreatedAt
            }).FirstOrDefaultAsync();
        return payload;
    }

    public async Task<List<Error>> ValidateSaveComponentSerial<T>(ComponentSerialInput input) where T : ComponentSerialInput {
        var errors = new List<Error>();

        var kitComponent = await context.KitComponents
            .Include(t => t.ProductionStation)
            .Include(t => t.Component)
            .FirstOrDefaultAsync(t => t.Id == input.KitComponentId);

        if (kitComponent == null) {
            errors.Add(new Error("KitComponentId", $"kit component not found: {input.KitComponentId}"));
            return errors;
        }

        if (kitComponent.RemovedAt != null) {
            errors.Add(new Error("KitComponentId", $"kit component removed: {input.KitComponentId}"));
            return errors;
        }

        // At least one serial number 
        if (String.IsNullOrWhiteSpace(input.Serial1) && String.IsNullOrWhiteSpace(input.Serial2)) {
            errors.Add(new Error("", "no serial numbers provided"));
            return errors;
        }

        // Serial numbers must be different
        if (!String.IsNullOrWhiteSpace(input.Serial1) && input.Serial1 == input.Serial2) {
            errors.Add(new Error("", "serial 1 and 2 are the same"));
            return errors;
        }

        // Some components require serial numbers to be re-formatted to conform to Ford web service rule
        // The reformatted serials will be saved
        // We will format them now, because we will look for duplicates further down.
        var formatResult = DcwsSerialFormatter.FormatSerialIfNeeded(kitComponent.Component.Code, new Serials(input.Serial1, input.Serial2));
        // set input.Serial1 to the formatted result before proceeding with the validation
        input = input with {
            Serial1 = formatResult.Serials.Serial1,
            Serial2 = formatResult.Serials.Serial2
        };

        if (!formatResult.Success) {
            errors.Add(new Error("", formatResult.Message));
            return errors;
        }


        // component serial entry for this kit component 
        var componentSerialForKitComponent = await context.ComponentSerials
            .Include(t => t.KitComponent).ThenInclude(t => t.Kit)
            .Include(t => t.KitComponent).ThenInclude(t => t.Component)
            .Include(t => t.KitComponent).ThenInclude(t => t.ProductionStation)
            .Where(t => t.KitComponent.Id == input.KitComponentId)
            .Where(t => t.RemovedAt == null)
            .FirstOrDefaultAsync();

        if (componentSerialForKitComponent != null && !input.Replace) {
            var vin = componentSerialForKitComponent.KitComponent.Kit.VIN;
            var stationCode = componentSerialForKitComponent.KitComponent.ProductionStation.Code;
            var componentCode = componentSerialForKitComponent.KitComponent.Component.Code;
            errors.Add(new Error("", $"component serial already captured for this component: {vin}-{stationCode}-{componentCode}"));
            return errors;
        }

        // serial no already in use by different kit component
        var componentSerials_with_same_serialNo = await context.ComponentSerials
            .Include(t => t.KitComponent).ThenInclude(t => t.Component)
            .Where(t => t.RemovedAt == null)
            // different component
            .Where(t => t.KitComponent.Id != kitComponent.Id)
            // exclude if same kit and same component code   
            // ....... Engine component code will be scanned muliple times)
            .Where(t => !(
                // same kit
                t.KitComponent.KitId == kitComponent.KitId
                &&
                // same component
                t.KitComponent.ComponentId == kitComponent.ComponentId
                )
            )
            // user could point scan serial 2 before serial 1, so we check for both
            .Where(t =>
                t.Serial1 == input.Serial1 && t.Serial2 == input.Serial2
                ||
                t.Serial1 == input.Serial2 && t.Serial2 == input.Serial1
            )
            .ToListAsync();


        if (componentSerials_with_same_serialNo.Any()) {
            errors.Add(new Error("", $"serial number already in use by aonther entry"));
            return errors;
        }

        /* MULTI STATION COMPONENT
        *  Some components serials should be captured repeatedly in consequitive production sations.
        *  They should not be captured out of order
        */

        var preeceedingRequiredComponentEntriesNotCaptured = await context.KitComponents
            .OrderBy(t => t.ProductionStation.Sequence)
            // save kit         
            .Where(t => t.Kit.Id == kitComponent.KitId)
            // same component id
            .Where(t => t.ComponentId == kitComponent.ComponentId)
            // preceeding target kit component
            .Where(t => t.ProductionStation.Sequence < kitComponent.ProductionStation.Sequence)
            // no captured serial entries
            .Where(t => !t.ComponentSerials.Any(u => u.RemovedAt == null))
            .Select(t => new {
                ProductionStationCode = t.ProductionStation.Code,
            })
            .ToListAsync();

        if (preeceedingRequiredComponentEntriesNotCaptured.Any()) {
            var statonCodes = preeceedingRequiredComponentEntriesNotCaptured
                .Select(t => t.ProductionStationCode)
                .Aggregate((a, b) => a + ", " + b);

            errors.Add(new Error("", $"serial numbers for prior stations not captured: {statonCodes}"));
            return errors;
        }

        /* MULTI STATION COMPONENT
        *  Must have matching serial numbers
        */

        var prior_kit_components_with_same_compoent_code_different_serial_numbers = await context.KitComponents
            .Include(t => t.ComponentSerials)
            .OrderBy(t => t.ProductionStation.Sequence)
            // save kit         
            .Where(t => t.Kit.Id == kitComponent.KitId)
            // same component id
            .Where(t => t.ComponentId == kitComponent.ComponentId)
            // preceeding target kit component
            .Where(t => t.ProductionStation.Sequence < kitComponent.ProductionStation.Sequence)
            // any active component serials that do not match input.Serial1, input.Serial2
            .Where(t =>
                t.ComponentSerials
                    .Where(t => t.RemovedAt == null)
                    .Any(u => u.Serial1 != input.Serial1 || u.Serial2 != input.Serial2)
            )
            .Select(t => new {
                t.ProductionStation.Sequence,
                t.ProductionStation.Code,
                t.ComponentSerials.Where(u => u.RemovedAt == null).First().Serial1,
                t.ComponentSerials.Where(u => u.RemovedAt == null).First().Serial2,
            })
            .ToListAsync();

        if (prior_kit_components_with_same_compoent_code_different_serial_numbers.Any()) {
            var entry = prior_kit_components_with_same_compoent_code_different_serial_numbers
                .OrderByDescending(t => t.Sequence)
                .Last();
            var text = $"serial does not match previous station: {entry.Code}, {entry.Serial1}, {entry.Serial2}";
            errors.Add(new Error("", text.Trim(' ', ',')));
            return errors;
        }

        return errors;
    }

    private static ComponentSerialInput SwapSerial(ComponentSerialInput input) {
        input = input with {
            Serial1 = input.Serial1 is null or "" ? "" : input.Serial1,
            Serial2 = input.Serial2 is null or "" ? "" : input.Serial2
        };

        if (input.Serial1.Trim().Length == 0) {
            return input with {
                KitComponentId = input.KitComponentId,
                Serial1 = input.Serial2,
                Serial2 = ""
            };
        }
        return input;
    }

    public async Task<ComponentSerial> ApplyComponentSerialFormat(Guid componentSerialId) {
        var cs = await context.ComponentSerials
            .Include(t => t.KitComponent).ThenInclude(t => t.Component)
            .FirstOrDefaultAsync(t => t.Id == componentSerialId);

        if (cs == null) {
            throw new Exception("Component serial not found");
        }

        if (cs.KitComponent.Component.Code != "EN") {
            throw new Exception("EN Component only");
        }

        var formatResult = DcwsSerialFormatter.FormatSerialIfNeeded(cs.KitComponent.Component.Code, new Serials(cs.Serial1, cs.Serial2));
        if (!formatResult.Success) {
            throw new Exception("Could not trasform: " + formatResult.Message);
        }
        cs.Original_Serial1 = cs.Serial1;
        cs.Serial1 = formatResult.Serials.Serial1;

        await context.SaveChangesAsync();
        return cs;
    }

    public async Task<BasicKitInfo?> GetBasicKitInfo(string vin) {
        var result = await context.Kits
            .Where(t => t.VIN == vin)
            .Select(t => new BasicKitInfo {
                KitNo = t.KitNo,
                VIN = t.VIN,
                LotNo = t.Lot.LotNo,
                ModelCode = t.Lot.Model.Code,
                ModelName = t.Lot.Model.Description
            }).FirstOrDefaultAsync();

        return result;
    }

    public async Task<KitComponentSerialInfo?> GetKitComponentSerialInfo(string kitNo, string componentCode) {
        var data = await context.KitComponents
            .Where(t => t.Kit.KitNo == kitNo && t.Component.Code == componentCode)
            .Select(t => new {
                KitComponentId = t.Id,
                RemvoedAt = t.RemovedAt,
                ComponentCode = t.Component.Code,
                ComponentName = t.Component.Name,
                StationSequence = t.ProductionStation.Sequence,
                StationCode = t.ProductionStation.Code,
                StationName = t.ProductionStation.Name,
                SerialCapture = t.ComponentSerials
                    .Where(k => k.RemovedAt == null)
                    .FirstOrDefault()
            }).ToListAsync();

        if (data.Count == 0) {
            return (KitComponentSerialInfo?)null;
        }

        var result = new KitComponentSerialInfo {
            ComponentCode = data[0].ComponentCode,
            ComponentName = data[0].ComponentCode,
            RemovedAt = data[0].RemvoedAt,
            Stations = data.OrderBy(t => t.StationSequence).Select(t => new StatcionSerialInfo {
                KitComponentId = t.KitComponentId,
                StationSequence = t.StationSequence,
                StationCode = t.StationCode,
                StationName = t.StationName,
                Serial1 = t.SerialCapture?.Serial1,
                Serial2 = t.SerialCapture?.Serial2,
                CreatedAt = t.SerialCapture?.CreatedAt,
                VerifiedAt = t.SerialCapture?.VerifiedAt
            }).ToList()
        };

        return result;
    }
}

