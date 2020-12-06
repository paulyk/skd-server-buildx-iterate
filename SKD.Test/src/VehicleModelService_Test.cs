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
        public async Task can_create_vehicle_model() {
            // setup
            var componentCode = "component_1";
            var stationCode = "station_1";
            Gen_Components(componentCode);
            Gen_ProductionStations(stationCode);


            var vehicleModel = new VehicleModelInput {
                Code = Util.RandomString(EntityFieldLen.VehicleModel_Code),
                Name = Util.RandomString(EntityFieldLen.VehicleModel_Name),
                ComponentStationDTOs = new List<ComponeentStationInput> {
                    new ComponeentStationInput {
                        ComponentCode = componentCode,
                        ProductionStationCode = stationCode,
                    }
                }
            };

            // test
            var service = new VehicleModelService(ctx);
            var before_count = await ctx.VehicleModels.CountAsync();

            // save
            var payload = await service.CreateVehicleModel(vehicleModel);

            // verify
            var after_count = await ctx.VehicleModels.CountAsync();
            Assert.Equal(before_count + 1, after_count);
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
            var payload = await service.CreateVehicleModel(model_1);

            //test
            var after_count = await ctx.VehicleModels.CountAsync();
            Assert.Equal(before_count, after_count);

            var errorCount = payload.Errors.Count();
            Assert.Equal(1, errorCount);
        }

        [Fact]
        public async Task cannot_add_duplicate_vehicle_model_code() {
            // setup
            Gen_Components("component_1");
            Gen_ProductionStations("station_1");

            var modelCode = Util.RandomString(EntityFieldLen.VehicleModel_Code);
            var modelName = Util.RandomString(EntityFieldLen.VehicleModel_Name);

            var model_1 = new VehicleModelInput {
                Code = modelCode,
                Name = modelName,
                ComponentStationDTOs = new List<ComponeentStationInput> {
                    new ComponeentStationInput {
                        ComponentCode = "component_1",
                        ProductionStationCode = "station_1"
                    }
                }
            };

            var model_2 = new VehicleModelInput {
                Code = modelCode,
                Name = Util.RandomString(EntityFieldLen.VehicleModel_Name),
                ComponentStationDTOs = new List<ComponeentStationInput> {
                    new ComponeentStationInput {
                        ComponentCode = "component_1",
                        ProductionStationCode = "station_1"
                    }
                }
            };


            // test
            var service = new VehicleModelService(ctx);
            await service.CreateVehicleModel(model_1);

            var payload = await service.CreateVehicleModel(model_2);

            var errorCount = payload.Errors.Count();
            var firstError = payload.Errors.Count() > 0
                ? payload.Errors.ToList()[0].Message
                : null;


            Assert.Equal(1, errorCount);
            Assert.Equal("duplicate code", firstError);
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
                ComponentStationDTOs = new List<ComponeentStationInput> {
                    new ComponeentStationInput {
                        ComponentCode = "component_1",
                        ProductionStationCode = "station_1"
                    }
                }
            };

            var model_2 = new VehicleModelInput {
                Code = Util.RandomString(EntityFieldLen.VehicleModel_Code),
                Name = modelName,
                ComponentStationDTOs = new List<ComponeentStationInput> {
                    new ComponeentStationInput {
                        ComponentCode = "component_1",
                        ProductionStationCode = "station_1"
                    }
                }
            };

            // test
            var service = new VehicleModelService(ctx);
            await service.CreateVehicleModel(model_1);

            var payload = await service.CreateVehicleModel(model_2);

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
                ComponentStationDTOs = new List<ComponeentStationInput> {
                    new ComponeentStationInput {
                        ComponentCode = component.Code,
                        ProductionStationCode = station.Code
                    },
                    new ComponeentStationInput {
                        ComponentCode = component.Code,
                        ProductionStationCode = station.Code
                    },
                }
            };

            // test
            var service = new VehicleModelService(ctx);
            var payload = await service.CreateVehicleModel(vehilceModel);

            // assert
            var errorCount = payload.Errors.Count();
            Assert.Equal(1, errorCount);
            var expectedErrorMessage = "duplicate component + production station entries";
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            Assert.Equal(expectedErrorMessage, errorMessage.Substring(0, expectedErrorMessage.Length));
        }
    }
}
