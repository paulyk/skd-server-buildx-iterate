using System;
using System.Collections.Generic;
using SKD.VCS.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.VCS.Test {
    public class VehicleServiceTest : TestBase {

        private SkdContext ctx;
        public VehicleServiceTest() {
            ctx = GetAppDbContext();
            GenerateSeedData();
        }

        [Fact]
        public async Task can_create_vehicle() {
            var service = new VehicleService(ctx);

            var vehicleModel = await ctx.VehicleModels.FirstOrDefaultAsync(t => t.Code == TestVehicleModel_Code);
            var vehicle = new Vehicle() {
                VIN = new String('1', EntityMaxLen.Vehicle_VIN),
                Model = vehicleModel,
                LotNo = "001",
                KitNo = "001"
            };

            var before_VehicleCount = await ctx.Vehicles.CountAsync();
            var payload = await service.CreateVehicle(vehicle);
            Assert.NotNull(payload.Entity);

            var after_VehicleCount = await ctx.Vehicles.CountAsync();
            Assert.True(after_VehicleCount == before_VehicleCount + 1, "Vehicle count unchanged after save");

            Assert.True(vehicleModel.ModelComponents.Count() == vehicle.VehicleComponents.Count(), "Vehicle components don't match vehicle model components");
        }

        [Fact]
        public async Task create_vehicle_with_no_model_return_payload_error() {
            // setup
            var service = new VehicleService(ctx);

            var vehicle = new Vehicle() {
                VIN = new String('1', EntityMaxLen.Vehicle_VIN),
                LotNo = "001",
                KitNo = "001"
            };

            // test
            var payload = await service.CreateVehicle(vehicle);

            // assert
            var errorCount = payload.Errors.Count();
            Assert.Equal(1, errorCount);
            Assert.Equal("Vehicle model not specified", payload.Errors.First().Message);
        }

        private string TestVehicleModel_Code = "FRNG20";

        private void GenerateSeedData() {

            var productionStations = new List<ProductionStation> {
                new ProductionStation() { Code = "STATION_1", Name = "Station name 1" },
                new ProductionStation() { Code = "STATION_2", Name = "Station name 2" },
            };
            ctx.ProductionStations.AddRange(productionStations);


            var components = new List<Component> {
                new Component() { Code = "COMP_1", Name = "Component name 1" },
                new Component() { Code = "COMP_2", Name = "Component name 2" },
            };
            ctx.Components.AddRange(components);

            var vehicleModel_1 = new VehicleModel() {
                Code = TestVehicleModel_Code,
                Name = "Ford Ranger 2.0",
                ModelComponents = components.Select((component, i) => new VehicleModelComponent() {
                    Component = component,
                    ProductionStation = productionStations[i]
                }).ToList()
            };

            ctx.VehicleModels.AddRange(vehicleModel_1);
            ctx.SaveChanges();
        }
    }
}
