using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {

    public class VehicleModelServiceTest : TestBase {

        public VehicleModelServiceTest() {
            context = GetAppDbContext();
        }

        [Fact]
        public async Task can_add_vehicle_model() {
            // setup
            var componentCodes = new string[] { "component_1", "component_2" };
            var stationCodes = new string[] { "station_1", "station_2" };
            Gen_Components(componentCodes);
            Gen_ProductionStations(stationCodes);

            var input = new VehicleModelInput {
                Code = Gen_VehicleModel_Code(),
                Name = Util.RandomString(EntityFieldLen.VehicleModel_Name),
                ComponentStationInputs = Enumerable.Range(0, componentCodes.Length)
                    .Select(i => new ComponentStationInput {
                        ComponentCode = componentCodes[i],
                        ProductionStationCode = stationCodes[i]
                    }).ToList()
            };

            var service = new VehicleModelService(context);

            // test
            var model_before_count = await context.VehicleModels.CountAsync();
            var component_before_count = await context.VehicleModelComponents.CountAsync();

            var payload = await service.SaveVehicleModel(input);

            // assert
            var model_after_count = await context.VehicleModels.CountAsync();
            Assert.Equal(model_before_count + 1, model_after_count);

            var component_after_count = await context.VehicleModelComponents.CountAsync();
            Assert.Equal(component_before_count + componentCodes.Length, component_after_count);
        }

        [Fact]
        public async Task cannot_save_if_duplicate_code_or_name() {
            // setup
            var componentCodes = new string[] { "component_1", "component_2" };
            var stationCodes = new string[] { "station_1", "station_2" };
            Gen_Components(componentCodes);
            Gen_ProductionStations(stationCodes);

            var input = new VehicleModelInput {
                Code = Gen_VehicleModel_Code(),
                Name = Util.RandomString(EntityFieldLen.VehicleModel_Name),
                ComponentStationInputs = Enumerable.Range(0, componentCodes.Length)
                    .Select(i => new ComponentStationInput {
                        ComponentCode = componentCodes[i],
                        ProductionStationCode = stationCodes[i]
                    }).ToList()
            };

            var service = new VehicleModelService(context);

            // test        
            await service.SaveVehicleModel(input);
            var model_count_1 = await context.VehicleModels.CountAsync();
            var model_component_count_1 = await context.VehicleModelComponents.CountAsync();

            var payload_2 = await service.SaveVehicleModel(input);
            var errors = payload_2.Errors.Select(t => t.Message).ToList();

            var ducplicateCode = errors.Any(error => error.StartsWith("duplicate code"));
            Assert.True(ducplicateCode);

            var ducplicateName = errors.Any(error => error.StartsWith("duplicate name"));
            Assert.True(ducplicateCode);
        }
        [Fact]
        public async Task can_modify_model_name() {
            // setup
            var componentCodes = new string[] { "component_1", "component_2" };
            var stationCodes = new string[] { "station_1", "station_2" };
            Gen_Components(componentCodes);
            Gen_ProductionStations(stationCodes);

            var input = new VehicleModelInput {
                Code = Gen_VehicleModel_Code(),
                Name = Gen_VehicleModel_Name(),
                ComponentStationInputs = Enumerable.Range(0, componentCodes.Length)
                    .Select(i => new ComponentStationInput {
                        ComponentCode = componentCodes[i],
                        ProductionStationCode = stationCodes[i]
                    }).ToList()
            };

            var service = new VehicleModelService(context);

            // test        
            await service.SaveVehicleModel(input);

            var model = await context.VehicleModels
                .Include(t => t.ModelComponents).ThenInclude(t => t.Component)
                .Include(t => t.ModelComponents).ThenInclude(t => t.ProductionStation)
            .FirstOrDefaultAsync(t => t.Code == input.Code);

            Assert.Equal(input.Name, model.Name);

            // modify name
            var input_2 = new VehicleModelInput {
                Id = model.Id,
                Code = model.Code,
                Name = Gen_VehicleModel_Name(),
                ComponentStationInputs = model.ModelComponents.Select(t => new ComponentStationInput {
                    ComponentCode = t.Component.Code,
                    ProductionStationCode = t.ProductionStation.Code
                }).ToList()
            };
            await service.SaveVehicleModel(input_2);

            model = await context.VehicleModels.FirstOrDefaultAsync(t => t.Code == input.Code);

            Assert.Equal(input_2.Name, model.Name);
        }

        [Fact]
        public async Task cannot_create_vehicle_model_witout_components() {
            // setup
            var service = new VehicleModelService(context);
            var before_count = await context.VehicleModels.CountAsync();

            var model_1 = new VehicleModelInput {
                Code = Util.RandomString(EntityFieldLen.VehicleModel_Code),
                Name = Util.RandomString(EntityFieldLen.VehicleModel_Name)
            };
            var payload = await service.SaveVehicleModel(model_1);

            //test
            var after_count = await context.VehicleModels.CountAsync();
            Assert.Equal(before_count, after_count);

            var errorCount = payload.Errors.Count();
            Assert.Equal(1, errorCount);
        }



        [Fact]
        public async Task cannot_add_vehicle_model_with_duplicate_component_station_entries() {
            // setup
            Gen_Components("component_1", "component_2");
            Gen_ProductionStations("station_1", "station_2");

            var component = context.Components.OrderBy(t => t.Code).First();
            var station = context.ProductionStations.OrderBy(t => t.Code).First();

            var vehilceModel = new VehicleModelInput {
                Code = "Model_1",
                Name = "Model Name",
                ComponentStationInputs = new List<ComponentStationInput> {
                    new ComponentStationInput {
                        ComponentCode = component.Code,
                        ProductionStationCode = station.Code
                    },
                    new ComponentStationInput {
                        ComponentCode = component.Code,
                        ProductionStationCode = station.Code
                    },
                }
            };

            // test
            var service = new VehicleModelService(context);
            var payload = await service.SaveVehicleModel(vehilceModel);

            // assert
            var errorCount = payload.Errors.Count();
            Assert.Equal(1, errorCount);
            var expectedErrorMessage = "duplicate component + production station entries";
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            Assert.Equal(expectedErrorMessage, errorMessage.Substring(0, expectedErrorMessage.Length));
        }
    }
}
