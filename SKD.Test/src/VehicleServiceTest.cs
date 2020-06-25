using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class VehicleServiceTest : TestBase {

        private AppDbContext ctx;
        public VehicleServiceTest() {
            ctx = GetAppDbContext();
            GenerateSeedData();
        }

        [Fact]
        public async Task can_create_vehicle() {
            var service = new VehicleService(ctx);

            var vehicle = new Vehicle() {
                VIN = new String('1', EntityMaxLen.Vehicle_VIN),
                Model = await ctx.VehicleModels.FirstOrDefaultAsync(t => t.Code == "FRNG20"),
                LotNo = "001",
                KitNo = "001"
            };

            var payload = await service.CreateVehicle(vehicle);
            if (payload.Entity == null) {
                Console.WriteLine("Entity is null!!!!!!!!!");
            } else {
                Console.WriteLine(("Vehicle VIN: " + payload.Entity.VIN));
            }

            Assert.NotNull(payload.Entity);
            
            var vehicleCount = await ctx.Vehicles.CountAsync();
            Assert.Equal(1, vehicleCount);
        }
        [Fact]
        public async Task create_vehicle_with_no_model_return_payload_error() {
            var service = new VehicleService(ctx);

            var vehicle = new Vehicle() {
                VIN = new String('1', EntityMaxLen.Vehicle_VIN),
                LotNo = "001",
                KitNo = "001"
            };

            var payload = await service.CreateVehicle(vehicle);

            var errorCount = payload.Errors.Count();
            Assert.Equal(1, errorCount);
            Assert.Equal("Vehicle model not specified", payload.Errors.First().Message);
        }

        private void GenerateSeedData() {

            var components = new List<Component>() {
                new Component() { Code = "COMP1", Name = "Component name 1", FordComponentType = "T1"},
                new Component() { Code = "COMP2", Name = "Component name 2", FordComponentType=  "T2"},
            };

            ctx.Components.AddRange(components);

            var vehicleModel_1 = new VehicleModel() {
                Code = "FRNG20",
                Name = "Ford Ranger 2.0",
                ComponentMappings = components.Select((component, i) => new VehicleModelComponent() {
                    Component = component,
                    Sequence = i + 1
                }).ToList()
            };

            ctx.VehicleModels.AddRange(vehicleModel_1);
            ctx.SaveChanges();
        }
    }
}
