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

            var vehicleModel = await ctx.VehicleModels
                .FirstOrDefaultAsync(t => t.Code == TestVehicleModel_Code);
            var dto = new VehicleDTO() {
                VIN = new String('1', EntityFieldLen.Vehicle_VIN),
                ModelCode = vehicleModel.Code,
                LotNo = new string('1', EntityFieldLen.Vehicle_LotNo),
                KitNo = new string('1', EntityFieldLen.Vehicle_KitNo)
            };

            var before_VehicleCount = await ctx.Vehicles.CountAsync();
            var payload = await service.CreateVehicle(dto);
            Assert.NotNull(payload.Entity);

            var after_VehicleCount = await ctx.Vehicles.CountAsync();
            Assert.True(after_VehicleCount == before_VehicleCount + 1, "Vehicle count unchanged after save");

            var vehicle = await ctx.Vehicles.FirstOrDefaultAsync(t => t.VIN == dto.VIN);
            var vehicleComponentCount = vehicle.VehicleComponents.Count();
            var modelComponentCount = vehicleModel.ModelComponents.Count();

            Console.WriteLine($"**** vehicle {vehicleComponentCount} model {modelComponentCount} ****");
            Assert.True(modelComponentCount == vehicleComponentCount, "Vehicle components don't match vehicle model components");
        }

        [Fact]
        public async Task create_vehicle_with_no_model_return_payload_error() {
            // setup
            var service = new VehicleService(ctx);

            var dto = new VehicleDTO() {
                VIN = new String('1', EntityFieldLen.Vehicle_VIN),
                LotNo = new string('1', EntityFieldLen.Vehicle_LotNo),
                KitNo = new string('1', EntityFieldLen.Vehicle_KitNo)
            };

            // test
            var payload = await service.CreateVehicle(dto);

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
