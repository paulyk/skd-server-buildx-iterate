

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SKD.Model;
using SKD.Common;

namespace SKD.Service {
    public class VehicleModelService {
        private readonly SkdContext context;

        public VehicleModelService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<VehicleModel>> Save(VehicleModelInput input) {
            var payload = new MutationPayload<VehicleModel>(null);
            payload.Errors = await ValidateSaveVehicleModel(input);
            if (payload.Errors.Any()) {
                return payload;
            }

            var vehicleModel = await context.VehicleModels
                .Include(t => t.ModelComponents).ThenInclude(t => t.Component)
                .Include(t => t.ModelComponents).ThenInclude(t => t.ProductionStation)
                .FirstOrDefaultAsync(t => t.Code == input.Code);

            // Add VehicleModel if null
            if (vehicleModel == null) {
                vehicleModel = new VehicleModel {
                    Code = input.Code,
                };
                context.VehicleModels.Add(vehicleModel);
            }
            vehicleModel.Name = input.Name;
            vehicleModel.ModelYear = input.ModelYear;
            vehicleModel.Model = input.Model;
            vehicleModel.Series = input.Series;
            vehicleModel.Body = input.Body;

            await UpdateComponents();

            // save
            await context.SaveChangesAsync();
            payload.Payload = vehicleModel;
            return payload;


            //
            async Task UpdateComponents() {
                // current_pairs, 
                var current_pairs = vehicleModel.ModelComponents.Any()
                    ? vehicleModel.ModelComponents.Select(t => new ComponentStationPair(
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


                var vehicle_model_components = vehicleModel.ModelComponents.Any()
                    ? vehicleModel.ModelComponents.ToList()
                    : new List<VehicleModelComponent>();

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
                        var modelComponent = new VehicleModelComponent {
                            Component = await context.Components.FirstOrDefaultAsync(t => t.Code == ta.ComponentCode),
                            ProductionStation = await context.ProductionStations.FirstOrDefaultAsync(t => t.Code == ta.StationCode)
                        };
                        vehicleModel.ModelComponents.Add(modelComponent);
                    }
                }

            }
        }


        public async Task<List<Error>> ValidateSaveVehicleModel<T>(T input) where T : VehicleModelInput {
            var errors = new List<Error>();

            VehicleModel existingVehicleModel = null;
            if (input.Id.HasValue) {
                existingVehicleModel = await context.VehicleModels.FirstOrDefaultAsync(t => t.Id == input.Id.Value);
                if (existingVehicleModel == null) {
                    errors.Add(ErrorHelper.Create<T>(t => t.Id, $"model not found: {input.Id}"));
                    return errors;
                }
            }

            // validate mddel code format
            if (input.Code.Trim().Length == 0) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, "code requred"));
            } else if (input.Code.Length > EntityFieldLen.VehicleModel_Code) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, $"exceeded code max length of {EntityFieldLen.VehicleModel_Code} characters "));
            }

            // validate model name format
            if (input.Name.Trim().Length == 0) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, "name requred"));
            } else if (input.Name.Length > EntityFieldLen.VehicleModel_Description) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, $"exceeded code max length of {EntityFieldLen.VehicleModel_Description} characters "));
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
            if (existingVehicleModel != null) {
                if (await context.VehicleModels.AnyAsync(t => t.Code == input.Code && t.Id != existingVehicleModel.Id)) {
                    errors.Add(ErrorHelper.Create<T>(t => t.Code, "duplicate code"));
                }
            } else {
                // adding a new component, so look for duplicate
                if (await context.VehicleModels.AnyAsync(t => t.Code == input.Code)) {
                    errors.Add(ErrorHelper.Create<T>(t => t.Code, "duplicate code"));
                }
            }

            // duplicate name
            if (existingVehicleModel != null) {
                if (await context.VehicleModels.AnyAsync(t => t.Name == input.Name && t.Id != existingVehicleModel.Id)) {
                    errors.Add(ErrorHelper.Create<T>(t => t.Name, "duplicate name"));
                }
            } else {
                // adding a new component, so look for duplicate
                if (await context.VehicleModels.AnyAsync(t => t.Name == input.Name)) {
                    errors.Add(ErrorHelper.Create<T>(t => t.Name, "duplicate name"));
                }
            }

            // components required
            if (input.ComponentStationInputs.Count() == 0) {
                errors.Add(ErrorHelper.Create<T>(t => t.ComponentStationInputs, "components requird"));
            }

            //  duplicate model code in same production stations
            var duplicate_component_station_entries = input.ComponentStationInputs
                .GroupBy(mc => new { mc.ComponentCode, mc.ProductionStationCode })
                .Select(g => new {
                    Key = g.Key,
                    Count = g.Count()
                }).Where(t => t.Count > 1).ToList();

            if (duplicate_component_station_entries.Count > 0) {
                var entries = duplicate_component_station_entries.Select(t => $"{t.Key.ComponentCode}:{t.Key.ProductionStationCode}");
                errors.Add(ErrorHelper.Create<T>(t => t.ComponentStationInputs, $"duplicate component + production station entries {String.Join(", ", entries)}"));
            }

            return errors;
        }


        public async Task<MutationPayload<VehicleModel>> CreateFromExisting(VehicleModelFromExistingInput input) {
            var payload = new MutationPayload<VehicleModel>(null);
            payload.Errors = await ValidateCreateFromExisting(input);
            if (payload.Errors.Any()) {
                return payload;
            }

            var existingModel = await context.VehicleModels
                .Include(t => t.ModelComponents)
                .FirstOrDefaultAsync(t => t.Code == input.ExistingModelCode);

            var newModel = new VehicleModel {
                Code = input.Code,
                Name = existingModel.Name,
                ModelYear = input.ModelYear,
                Model = existingModel.Model,
                Body = existingModel.Body,
                Series = existingModel.Series,
                ModelComponents = existingModel.ModelComponents.Select(mc => new VehicleModelComponent {
                    ComponentId = mc.ComponentId,
                    ProductionStationId = mc.ProductionStationId
                }).ToList()
            };

            context.VehicleModels.Add(newModel);
            await context.SaveChangesAsync();
            payload.Payload = newModel;

            return payload;

        }

        public async Task<List<Error>> ValidateCreateFromExisting(VehicleModelFromExistingInput input) {
            var errors = new List<Error>();
            var validator = new Validator();

            var codeAlreadyTaken = await context.VehicleModels.AnyAsync(t => t.Code == input.Code);
            if (codeAlreadyTaken) {
                errors.Add(new Error("", $"Model code already exists: {input.Code}"));
                return errors;
            }

            var existingModel = await context.VehicleModels.FirstOrDefaultAsync(t => t.Code == input.ExistingModelCode);
            if (existingModel == null) {
                errors.Add(new Error("", $"Existing model PCV not found: {input.ExistingModelCode}"));
                return errors;
            }

            if (!validator.Valid_PCV(input.Code)) {
                errors.Add(new Error("", $"invalid PCV code: {input.Code}"));
            }


            return errors;
        }

        public async Task<MutationPayload<Kit>> SyncKfitModelComponents(string kitNo) {
            var payload = new MutationPayload<Kit>(null);
            payload.Errors = await ValidateSyncKitModelComponents(kitNo);
            if (payload.Errors.Any()) {
                return payload;
            }

            var kit = await context.Kits
                .Include(t => t.KitComponents).ThenInclude(t => t.Component)
                .Include(t => t.KitComponents).ThenInclude(t => t.ProductionStation)
                .FirstOrDefaultAsync(t => t.KitNo == kitNo);
            payload.Payload = kit;

            var diff = await GetKitModelComponentDiff(kitNo);

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

            return payload;
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

            var planBuildEventType = await context.KitTimelineEventTypes.FirstOrDefaultAsync(t => t.Code == TimeLineEventCode.BUILD_COMPLETED);
            var latestTimelineEvent = kit.TimelineEvents
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefault();

            if (latestTimelineEvent != null && latestTimelineEvent.EventType.Sequence >= planBuildEventType.Sequence) {
                errors.Add(new Error("", "cannot update kit components if build compplete"));
                return errors;
            }

            return errors;
        }


        #region kit model component diff
        public record ComponentStationPair(string ComponentCode, string StationCode);
        public record KitModelComponentDiff(
            List<ComponentStationPair> InModelButNoKit,
            List<ComponentStationPair> InKitButNoModel
        );
        public async Task<KitModelComponentDiff> GetKitModelComponentDiff(string kitNo) {

            var kit = await context.Kits
                .Include(t => t.Lot)
                .FirstOrDefaultAsync(t => t.KitNo == kitNo);

            var kitComponents = await context.KitComponents
                .Where(t => t.Kit.KitNo == kitNo)
                .Where(t => t.RemovedAt == null)
                .Select(t => new ComponentStationPair(
                    t.Component.Code,
                    t.ProductionStation.Code
                )).ToListAsync();

            var modelComponents = await context.VehicleModelComponents
                .Where(t => t.VehicleModelId == kit.Lot.ModelId)
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
}