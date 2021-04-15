

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.Model {
    public class VehicleModelService {
        private readonly SkdContext context;

        public VehicleModelService(SkdContext ctx) {
            this.context = ctx;
        }
        public async Task<MutationPayload<VehicleModel>> SaveVehicleModel(VehicleModelInput input) {
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

            // current_pairs, 
            var current_pairs = vehicleModel.ModelComponents.Any()
                ? vehicleModel.ModelComponents.Select(t => new Pair {
                    Component = t.Component.Code,
                    Station = t.ProductionStation.Code
                }).ToList()
                : new List<Pair>();

            // incomming_pairs
            var incomming_pairts = input.ComponentStationInputs.Select(t => new Pair {
                Component = t.ComponentCode,
                Station = t.ProductionStationCode
            }).ToList();

            // to_remove, to_add
            var to_remove = current_pairs.Except(incomming_pairts).ToList();
            var to_add = incomming_pairts.Except(current_pairs).ToList();


            var vehicle_model_components = vehicleModel.ModelComponents.Any()
                ? vehicleModel.ModelComponents.ToList()
                : new List<VehicleModelComponent>();

            // remove
            foreach (var entry in vehicle_model_components
                .Where(t => t.RemovedAt == null)
                .Where(t => to_remove.Any(tr => tr.Component == t.Component.Code && tr.Station == t.ProductionStation.Code))
                .ToList()) {
                entry.RemovedAt = DateTime.UtcNow;
            }

            // add             
            foreach (var ta in to_add) {
                var existing = vehicle_model_components.FirstOrDefault(t => t.Component.Code == ta.Component && t.ProductionStation.Code == ta.Station);
                if (existing != null) {
                    existing.RemovedAt = null;
                } else {
                    var modelComponent = new VehicleModelComponent {
                        Component = await context.Components.FirstOrDefaultAsync(t => t.Code == ta.Component),
                        ProductionStation = await context.ProductionStations.FirstOrDefaultAsync(t => t.Code == ta.Station)
                    };
                    vehicleModel.ModelComponents.Add(modelComponent);
                }
            }

            // save
            await context.SaveChangesAsync();
            payload.Entity = vehicleModel;
            return payload;
        }

        struct Pair {
            public string Component { get; set; }
            public string Station { get; set; }
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
            } else if (input.Name.Length > EntityFieldLen.VehicleModel_Name) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, $"exceeded code max length of {EntityFieldLen.VehicleModel_Name} characters "));
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

    }
}