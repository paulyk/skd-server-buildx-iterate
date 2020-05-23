using System;
using System.Collections.Generic;
using VT.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace VT.Test {
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

            var result = await service.CreateVehicle(vehicle);
            Assert.NotNull(result.Vehicle);

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

            var result = await service.CreateVehicle(vehicle);
            Assert.Equal(1, result.Errors.Count);

            Assert.Equal("Vehicle model not specified", result.Errors.First().Message);
        }

       

        private void GenerateSeedData() {

            var components = new List<Component>() {
                new Component() { Code = "COMP1", Name = "Component name 1", Type = "T1"},
                new Component() { Code = "COMP2", Name = "Component name 2", Type=  "T2"},
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
