

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.VCS.Model {
    public class VehicleModelService {
        private readonly SkdContext context;

        public VehicleModelService(SkdContext ctx) {
            this.context = ctx;
        }
        public async Task<MutationPayload<VehicleModel>> CreateVehicleModel(VehicleModelDTO dto) {
            var vehicleModel = new VehicleModel {
                Code = dto.Code,
                Name = dto.Name,
                ModelComponents = dto.ComponentStationDTOs.Select(mc => {
                    var component = context.Components.FirstOrDefault(t => t.Code == mc.ComponentCode);
                    var station = context.ProductionStations.FirstOrDefault(t => t.Code == mc.ProductionStationCode);
                    return new VehicleModelComponent {
                        ComponentId = component.Id,
                        Component = component,
                        ProductionStationId = station.Id,
                        ProductionStation = station
                    };
                }).ToList()
            };

            var payload = new MutationPayload<VehicleModel>(vehicleModel);
            payload.Errors = await ValidateCreateVehicleModel(vehicleModel);
              
            if (payload.Errors.Any()) {
                return payload;
            }

            context.VehicleModels.Add(vehicleModel);

            // save
            await context.SaveChangesAsync();
            payload.Entity = vehicleModel;
            return payload;
        }

        public async Task<List<Error>> ValidateCreateVehicleModel<T>(T model) where T : VehicleModel {
            var errors = new List<Error>();

            // validate code 
            if (model.Code.Trim().Length == 0) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, "code requred"));
            } else if (model.Code.Length > EntityFieldLen.VehicleModel_Code) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, $"exceeded code max length of {EntityFieldLen.VehicleModel_Code} characters "));
            }

            // validate name
            if (model.Name.Trim().Length == 0) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, "name requred"));
            } else if (model.Name.Length > EntityFieldLen.VehicleModel_Name) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, $"exceeded code max length of {EntityFieldLen.VehicleModel_Name} characters "));
            }

            // duplicate code
            if (await context.VehicleModels.AnyAsync(t => t.Id != model.Id && t.Code == model.Code)) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, "duplicate code"));
            }
            // duplicate name
            if (await context.VehicleModels.AnyAsync(t => t.Id != model.Id && t.Name == model.Name)) {
                errors.Add(ErrorHelper.Create<T>(t => t.Name, "duplicate name"));
            }

            // components required
            if (model.ModelComponents.Count() == 0) {
                errors.Add(ErrorHelper.Create<T>(t => t.ModelComponents, "components requird"));
            }

            //  duplicate model code in same production stations
            var duplicate_component_station_entries = model.ModelComponents
                .GroupBy(mc => new { mc.Component, mc.ProductionStation })
                .Select(g => new {
                    Key = g.Key,
                    Count = g.Count()
                }).Where(t => t.Count > 1).ToList();

            if (duplicate_component_station_entries.Count > 0) {
                var entries = duplicate_component_station_entries.Select(t => $"{t.Key.Component.Code}:{t.Key.ProductionStation.Code}");
                errors.Add(ErrorHelper.Create<T>(t => t.ModelComponents, $"duplicate component + production station entries {String.Join(", ",entries)}"));
            }


            return errors;
        }

    }
}