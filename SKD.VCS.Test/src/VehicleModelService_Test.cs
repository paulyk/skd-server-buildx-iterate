using System;
using System.Collections.Generic;
using SKD.VCS.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.VCS.Test {

    public class VehicleModelServiceTest : TestBase {

        private SkdContext ctx;
        public VehicleModelServiceTest() {
            ctx = GetAppDbContext();
            GenerateSeedData();
        }

        [Fact]
        public async Task can_create_vehicle_model() {
            // setup
            var components = await ctx.Components.ToListAsync();
            var productionStations = await ctx.ProductionStations.ToListAsync();

            var vehicleModel = new VehicleModelDTO {
                Code = Util.RandomString(EntityFieldLen.VehicleModel_Code),
                Name = Util.RandomString(EntityFieldLen.VehicleModel_Name),
                Components = new int[STATION_COMPONENT_COUNT].ToList()
                    .Select((v, i) => new ComponeentStationDTO {
                        ComponentCode = components[i].Code,
                        ProductionStationCode = productionStations[i].Code
                    }).ToList()
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

            var model_1 = new VehicleModelDTO {
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
            var service = new VehicleModelService(ctx);

            var components = await ctx.Components.ToListAsync();
            var productionStations = await ctx.ProductionStations.ToListAsync();

            var modelCode = Util.RandomString(EntityFieldLen.VehicleModel_Code);
            var modelName = Util.RandomString(EntityFieldLen.VehicleModel_Name);

            var model_1 = new VehicleModelDTO {
                Code = modelCode,
                Name = modelName,
                Components = new int[STATION_COMPONENT_COUNT].ToList()
                    .Select((v, i) => new ComponeentStationDTO {
                        ComponentCode = components[i].Code,
                        ProductionStationCode = productionStations[i].Code
                    }).ToList()
            };
            await service.CreateVehicleModel(model_1);

            // test
            var model_2 = new VehicleModelDTO {
                Code = modelCode,
                Name = Util.RandomString(EntityFieldLen.VehicleModel_Name),
                Components = new int[STATION_COMPONENT_COUNT].ToList()
                    .Select((v, i) => new ComponeentStationDTO {
                        ComponentCode = components[i].Code,
                        ProductionStationCode = productionStations[i].Code
                    }).ToList()
            };
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
            var service = new VehicleModelService(ctx);

            var components = await ctx.Components.ToListAsync();
            var productionStations = await ctx.ProductionStations.ToListAsync();

            var modelCode = Util.RandomString(EntityFieldLen.VehicleModel_Code);
            var modelName = Util.RandomString(EntityFieldLen.VehicleModel_Name);

            var model_1 = new VehicleModelDTO {
                Code = modelCode,
                Name = modelName,
                Components = new int[STATION_COMPONENT_COUNT].ToList()
                    .Select((v, i) => new ComponeentStationDTO {
                        ComponentCode = components[i].Code,
                        ProductionStationCode = productionStations[i].Code
                    }).ToList()
            };
            await service.CreateVehicleModel(model_1);

            // test
            var model_2 = new VehicleModelDTO {
                Code = Util.RandomString(EntityFieldLen.VehicleModel_Code),
                Name = modelName,
                Components = new int[STATION_COMPONENT_COUNT].ToList()
                    .Select((v, i) => new ComponeentStationDTO {
                        ComponentCode = components[i].Code,
                        ProductionStationCode = productionStations[i].Code
                    }).ToList()
            };
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
            var component = ctx.Components.OrderBy(t => t.Code).First();
            var station = ctx.ProductionStations.OrderBy(t => t.Code).First();

            var vehilceModel = new VehicleModelDTO {
                Code = "Model_1",
                Name = "Model Name",
                Components = new List<ComponeentStationDTO> {
                    new ComponeentStationDTO {
                        ComponentCode = component.Code,
                        ProductionStationCode = station.Code
                    },
                    new ComponeentStationDTO {
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

        }
        private int STATION_COMPONENT_COUNT = 2;
        private void GenerateSeedData() {
           Gen_ProductionStations(ctx,"station_1", "station_2");
           Gen_Components(ctx, "component_1", "component_2");
        }
    }
}
