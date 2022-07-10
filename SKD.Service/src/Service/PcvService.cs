#nullable enable

namespace SKD.Service;
public class PcvService {
    private readonly SkdContext context;

    public PcvService(SkdContext ctx) {
        this.context = ctx;
    }

    public async Task<MutationResult<PCV>> Save(PcvInput input) {
        MutationResult<PCV> result = new();
        result.Errors = await ValidateSavePcv(input);
        if (result.Errors.Any()) {
            return result;
        }

        var pcv = await context.Pcvs
            .Include(t => t.PcvComponents).ThenInclude(t => t.Component)
            .Include(t => t.PcvComponents).ThenInclude(t => t.ProductionStation)
            .FirstOrDefaultAsync(t => t.Code == input.Code);

        // Add Pcv if null
        if (pcv == null) {
            pcv = new PCV {
                Code = input.Code,
            };
            context.Pcvs.Add(pcv);
        }
        pcv.Description = input.Description;
        pcv.ModelYear = input.ModelYear;
        pcv.Model = input.Model;
        pcv.Series = input.Series;
        pcv.Body = input.Body;

        await UpdateComponents();

        // save
        await context.SaveChangesAsync();
        result.Payload = pcv;
        return result;


        //
        async Task UpdateComponents() {
            // current_pairs, 
            var current_pairs = pcv.PcvComponents.Any()
                ? pcv.PcvComponents.Select(t => new ComponentStationPair(
                    ComponentCode: t.Component.Code,
                    StationCode: t.ProductionStation.Code
                  )).ToList()
                : new List<ComponentStationPair>();

            // incomming_pairs
            var incomming_pairts = input.ComponentStationInputs.Select(t => new ComponentStationPair(
                ComponentCode: t.ComponentCode,
                StationCode: t.ProductionStationCode
            )).ToList();

            // to_remove, to_add
            var to_remove = current_pairs.Except(incomming_pairts).ToList();
            var to_add = incomming_pairts.Except(current_pairs).ToList();


            var vehicle_model_components = pcv.PcvComponents.Any()
                ? pcv.PcvComponents.ToList()
                : new List<PcvComponent>();

            // remove
            foreach (var entry in vehicle_model_components
                .Where(t => t.RemovedAt == null)
                .Where(t => to_remove.Any(tr => tr.ComponentCode == t.Component.Code && tr.StationCode == t.ProductionStation.Code))
                .ToList()) {
                entry.RemovedAt = DateTime.UtcNow;
            }

            // add             
            foreach (var ta in to_add) {
                var existing = vehicle_model_components
                    .Where(t => t.Component.Code == ta.ComponentCode && t.ProductionStation.Code == ta.StationCode)
                    .FirstOrDefault();
                if (existing != null) {
                    existing.RemovedAt = null;
                } else {
                    var modelComponent = new PcvComponent {
                        Component = await context.Components.FirstOrDefaultAsync(t => t.Code == ta.ComponentCode),
                        ProductionStation = await context.ProductionStations.FirstOrDefaultAsync(t => t.Code == ta.StationCode)
                    };
                    pcv.PcvComponents.Add(modelComponent);
                }
            }

        }
    }


    public async Task<List<Error>> ValidateSavePcv<T>(T input) where T : PcvInput {
        var errors = new List<Error>();

        PCV? existingPcv = null;
        if (input.Id.HasValue) {
            existingPcv = await context.Pcvs.FirstOrDefaultAsync(t => t.Id == input.Id.Value);
            if (existingPcv == null) {
                errors.Add(ErrorHelper.Create<T>(t => t.Id, $"PCV not found: {input.Id}"));
                return errors;
            }
        }

        // validate mddel code format
        if (input.Code.Trim().Length == 0) {
            errors.Add(ErrorHelper.Create<T>(t => t.Code, "code requred"));
        } else if (input.Code.Length > EntityFieldLen.Pcv_Code) {
            errors.Add(ErrorHelper.Create<T>(t => t.Code, $"exceeded code max length of {EntityFieldLen.Pcv_Code} characters "));
        }

        // validate pcv name format
        if (input.Description.Trim().Length == 0) {
            errors.Add(ErrorHelper.Create<T>(t => t.Code, "name requred"));
        } else if (input.Description.Length > EntityFieldLen.Pcv_Description) {
            errors.Add(ErrorHelper.Create<T>(t => t.Code, $"exceeded code max length of {EntityFieldLen.Pcv_Description} characters "));
        }

        // unknown componet codes
        var existingComponentCodes = await context.Components.Select(t => t.Code).ToListAsync();
        var modelComponentCodes = input.ComponentStationInputs.Select(t => t.ComponentCode).ToList();
        var missingComponentCodes = modelComponentCodes.Except(existingComponentCodes);
        if (missingComponentCodes.Any()) {
            errors.Add(ErrorHelper.Create<T>(t => t.Code, $"unknown component codes {String.Join(", ", missingComponentCodes)}"));
        }

        // unknown production station codes
        var existingStationCodes = await context.Components.Select(t => t.Code).ToListAsync();
        var modelStationCodes = input.ComponentStationInputs.Select(t => t.ComponentCode).ToList();
        var missingStationCodes = modelStationCodes.Except(existingStationCodes);
        if (missingStationCodes.Any()) {
            errors.Add(ErrorHelper.Create<T>(t => t.Code, $"unknown production station codes {String.Join(", ", missingStationCodes)}"));
        }

        // 
        if (existingPcv != null) {
            if (await context.Pcvs.AnyAsync(t => t.Code == input.Code && t.Id != existingPcv.Id)) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, "duplicate code"));
            }
        } else {
            // adding a new component, so look for duplicate
            if (await context.Pcvs.AnyAsync(t => t.Code == input.Code)) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, "duplicate code"));
            }
        }

        // duplicate name
        if (existingPcv != null) {
            if (await context.Pcvs.AnyAsync(t => t.Description == input.Description && t.Id != existingPcv.Id)) {
                errors.Add(ErrorHelper.Create<T>(t => t.Description, "duplicate name"));
            }
        } else {
            // adding a new component, so look for duplicate
            if (await context.Pcvs.AnyAsync(t => t.Description == input.Description)) {
                errors.Add(ErrorHelper.Create<T>(t => t.Description, "duplicate name"));
            }
        }

        // components required
        if (input.ComponentStationInputs.Count == 0) {
            errors.Add(ErrorHelper.Create<T>(t => t.ComponentStationInputs, "components requird"));
        }

        //  duplicate pcv code in same production stations
        var duplicate_component_station_entries = input.ComponentStationInputs
            .GroupBy(mc => new { mc.ComponentCode, mc.ProductionStationCode })
            .Select(g => new {
                g.Key,
                Count = g.Count()
            }).Where(t => t.Count > 1).ToList();

        if (duplicate_component_station_entries.Count > 0) {
            var entries = duplicate_component_station_entries.Select(t => $"{t.Key.ComponentCode}:{t.Key.ProductionStationCode}");
            errors.Add(ErrorHelper.Create<T>(t => t.ComponentStationInputs, $"duplicate component + production station entries {String.Join(", ", entries)}"));
        }

        return errors;
    }


    public async Task<MutationResult<PCV>> CreateFromExisting(PcvFromExistingInput input) {
        MutationResult<PCV> result = new();
        result.Errors = await ValidateCreateFromExisting(input);
        if (result.Errors.Any()) {
            return result;
        }

        var existingModel = await context.Pcvs
            .Include(t => t.PcvComponents)
            .FirstAsync(t => t.Code == input.ExistingModelCode);

        var newModel = new PCV {
            Code = input.Code,
            Description = existingModel.Description,
            ModelYear = input.ModelYear,
            Model = existingModel.Model,
            Body = existingModel.Body,
            Series = existingModel.Series,
            PcvComponents = existingModel.PcvComponents.Select(mc => new PcvComponent {
                ComponentId = mc.ComponentId,
                ProductionStationId = mc.ProductionStationId
            }).ToList()
        };

        context.Pcvs.Add(newModel);
        await context.SaveChangesAsync();
        result.Payload = newModel;

        return result;

    }

    public async Task<List<Error>> ValidateCreateFromExisting(PcvFromExistingInput input) {
        var errors = new List<Error>();


        var codeAlreadyTaken = await context.Pcvs.AnyAsync(t => t.Code == input.Code);
        if (codeAlreadyTaken) {
            errors.Add(new Error("", $"Model code already exists: {input.Code}"));
            return errors;
        }

        var existingModel = await context.Pcvs.FirstOrDefaultAsync(t => t.Code == input.ExistingModelCode);
        if (existingModel == null) {
            errors.Add(new Error("", $"Existing PCV not found: {input.ExistingModelCode}"));
            return errors;
        }

        if (!Validator.Valid_PCV(input.Code)) {
            errors.Add(new Error("", $"invalid PCV code: {input.Code}"));
        }

        return errors;
    }

    public async Task<MutationResult<Kit>> SyncKfitPcvComponents(string kitNo) {
        MutationResult<Kit> result = new(null);
        result.Errors = await ValidateSyncKitModelComponents(kitNo);
        if (result.Errors.Any()) {
            return result;
        }

        var kit = await context.Kits
            .Include(t => t.KitComponents).ThenInclude(t => t.Component)
            .Include(t => t.KitComponents).ThenInclude(t => t.ProductionStation)
            .FirstAsync(t => t.KitNo == kitNo);
        result.Payload = kit;

        var diff = await GetKitPcvComponentDiff(kitNo);

        if (diff.InKitButNoModel.Any()) {
            // remove
            kit.KitComponents.ToList()
                .Where(t => t.RemovedAt == null)
                .Where(t => diff.InKitButNoModel
                    .Any(d => d.ComponentCode == t.Component.Code && d.StationCode == t.ProductionStation.Code))
                .ToList()
                .ForEach(kc => {
                    kc.RemovedAt = DateTime.UtcNow;
                });
        }
        if (diff.InModelButNoKit.Any()) {
            foreach (var entry in diff.InModelButNoKit) {
                // chekc if kit component alread exists.
                var existingKitComponent = kit.KitComponents
                    .Where(t => t.Component.Code == entry.ComponentCode && t.ProductionStation.Code == entry.StationCode)
                    .FirstOrDefault();

                if (existingKitComponent != null) {
                    existingKitComponent.RemovedAt = null;
                } else {
                    kit.KitComponents.Add(new KitComponent {
                        Component = await context.Components.FirstAsync(t => t.Code == entry.ComponentCode),
                        ProductionStation = await context.ProductionStations.FirstOrDefaultAsync(t => t.Code == entry.StationCode)
                    });
                }
            }
        }

        await context.SaveChangesAsync();

        return result;
    }

    public async Task<List<Error>> ValidateSyncKitModelComponents(string kitNo) {
        var errors = new List<Error>();

        var kit = await context.Kits
            .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
            .FirstOrDefaultAsync(t => t.KitNo == kitNo);

        if (kit == null) {
            errors.Add(new Error("", "Kit not found for " + kitNo));
            return errors;
        }

        if (kit.RemovedAt != null) {
            errors.Add(new Error("", "kit removed"));
            return errors;
        }

        var planBuildEventType = await context.KitTimelineEventTypes.FirstAsync(t => t.Code == TimeLineEventCode.BUILD_COMPLETED);
        var latestTimelineEvent = kit.TimelineEvents
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefault();

        if (latestTimelineEvent != null && latestTimelineEvent.EventType.Sequence >= planBuildEventType.Sequence) {
            errors.Add(new Error("", "cannot update kit components if build compplete"));
            return errors;
        }

        return errors;
    }


    #region kit pcv component diff
    public record ComponentStationPair(string ComponentCode, string StationCode);
    public record KitModelComponentDiff(
        List<ComponentStationPair> InModelButNoKit,
        List<ComponentStationPair> InKitButNoModel
    );
    public async Task<KitModelComponentDiff> GetKitPcvComponentDiff(string kitNo) {

        var kit = await context.Kits
            .Include(t => t.Lot)
            .FirstAsync(t => t.KitNo == kitNo);

        var kitComponents = await context.KitComponents
            .Where(t => t.Kit.KitNo == kitNo)
            .Where(t => t.RemovedAt == null)
            .Select(t => new ComponentStationPair(
                t.Component.Code,
                t.ProductionStation.Code
            )).ToListAsync();

        var modelComponents = await context.PcvComponents
            .Where(t => t.PcvId == kit.Lot.ModelId)
            .Where(t => t.RemovedAt == null)
            .Select(t => new ComponentStationPair(
                t.Component.Code,
                t.ProductionStation.Code
            )).ToListAsync();

        return new KitModelComponentDiff(
            InModelButNoKit: modelComponents.Except(kitComponents).ToList(),
            InKitButNoModel: kitComponents.Except(modelComponents).ToList()
        );
    }
    #endregion
}