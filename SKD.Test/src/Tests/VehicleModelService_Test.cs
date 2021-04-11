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
            ctx = GetAppDbContext();
        }

        [Fact]
        public async Task can_add_vehicle_model() {
            // setup
            var componentCodes = new string[] { "component_1", "component_2" };
            var stationCodes = new string[] { "station_1", "station_2" };
            Gen_Components(componentCodes);
            Gen_ProductionStations(stationCodes);

            var vehicleModel = new VehicleModelInput {
                Code = Gen_VehicleModel_Code(),
                Name = Util.RandomString(EntityFieldLen.VehicleModel_Name),
                ComponentStationInputs = Enumerable.Range(0, componentCodes.Length)
                    .Select(i => new ComponentStationInput {
                        ComponentCode = componentCodes[i],
                        ProductionStationCode = stationCodes[i]
                    }).ToList()
            };

            var service = new VehicleModelService(ctx);

            // test
            var vehicle_model_before_count = await ctx.VehicleModels.CountAsync();
            var vehicle_model_component_before_count = await ctx.VehicleModelComponents.CountAsync();

            var payload = await service.SaveVehicleModel(vehicleModel);

            // assert
            var vehicle_model_after_count = await ctx.VehicleModels.CountAsync();
            Assert.Equal(vehicle_model_before_count + 1, vehicle_model_after_count);

            var vehicle_model_component_after_count = await ctx.VehicleModelComponents.CountAsync();
            Assert.Equal(vehicle_model_component_before_count + componentCodes.Length, vehicle_model_component_after_count);
        }

        [Fact]
        public async Task add_vehicle_model_twice_has_no_side_effect() {
            // setup
            var componentCodes = new string[] { "component_1", "component_2" };
            var stationCodes = new string[] { "station_1", "station_2" };
            Gen_Components(componentCodes);
            Gen_ProductionStations(stationCodes);

            var vehicleModel = new VehicleModelInput {
                Code = Gen_VehicleModel_Code(),
                Name = Util.RandomString(EntityFieldLen.VehicleModel_Name),
                ComponentStationInputs = Enumerable.Range(0, componentCodes.Length)
                    .Select(i => new ComponentStationInput {
                        ComponentCode = componentCodes[i],
                        ProductionStationCode = stationCodes[i]
                    }).ToList()
            };

            var service = new VehicleModelService(ctx);

            // test        
            await service.SaveVehicleModel(vehicleModel);
            var model_count_1 = await ctx.VehicleModels.CountAsync();
            var model_component_count_1 = await ctx.VehicleModelComponents.CountAsync();

            var payload_2 = await service.SaveVehicleModel(vehicleModel);
            var model_count_2 = await ctx.VehicleModels.CountAsync();
            var model_component_count_2 = await ctx.VehicleModelComponents.CountAsync();

            Assert.Equal(model_count_1, model_count_2);
            Assert.Equal(model_component_count_1, model_component_count_2);
        }

        [Fact]
        public async Task cannot_create_vehicle_model_witout_components() {

            // setup
            var service = new VehicleModelService(ctx);
            var before_count = await ctx.VehicleModels.CountAsync();

            var model_1 = new VehicleModelInput {
                Code = Util.RandomString(EntityFieldLen.VehicleModel_Code),
                Name = Util.RandomString(EntityFieldLen.VehicleModel_Name)
            };
            var payload = await service.SaveVehicleModel(model_1);

            //test
            var after_count = await ctx.VehicleModels.CountAsync();
            Assert.Equal(before_count, after_count);

            var errorCount = payload.Errors.Count();
            Assert.Equal(1, errorCount);
        }



        [Fact]
        public async Task cannot_add_duplicate_vehicle_model_name() {
            // setup
            Gen_Components("component_1");
            Gen_ProductionStations("station_1");

            var components = await ctx.Components.ToListAsync();
            var productionStations = await ctx.ProductionStations.ToListAsync();

            var modelCode = Util.RandomString(EntityFieldLen.VehicleModel_Code);
            var modelName = Util.RandomString(EntityFieldLen.VehicleModel_Name);

            var model_1 = new VehicleModelInput {
                Code = modelCode,
                Name = modelName,
                ComponentStationInputs = new List<ComponentStationInput> {
                    new ComponentStationInput {
                        ComponentCode = "component_1",
                        ProductionStationCode = "station_1"
                    }
                }
            };

            var model_2 = new VehicleModelInput {
                Code = Util.RandomString(EntityFieldLen.VehicleModel_Code),
                Name = modelName,
                ComponentStationInputs = new List<ComponentStationInput> {
                    new ComponentStationInput {
                        ComponentCode = "component_1",
                        ProductionStationCode = "station_1"
                    }
                }
            };

            // test
            var service = new VehicleModelService(ctx);
            await service.SaveVehicleModel(model_1);

            var payload = await service.SaveVehicleModel(model_2);

            var errorCount = payload.Errors.Count();
            var firstError = payload.Errors.Count() > 0
                ? payload.Errors.ToList()[0].Message
                : null;


            Assert.Equal(1, errorCount);
            Assert.Equal("duplicate name", firstError);
        }


        [Fact]
        public async Task cannot_add_vehicle_model_with_duplicate_component_station_entries() {
            // setup
            Gen_Components("component_1", "component_2");
            Gen_ProductionStations("station_1", "station_2");

            var component = ctx.Components.OrderBy(t => t.Code).First();
            var station = ctx.ProductionStations.OrderBy(t => t.Code).First();

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
            var service = new VehicleModelService(ctx);
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
