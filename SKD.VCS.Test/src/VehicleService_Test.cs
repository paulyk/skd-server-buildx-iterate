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
            // setup
            var service = new VehicleService(ctx);

            var vehicleModel = await ctx.VehicleModels
                .FirstOrDefaultAsync(t => t.Code == TestVehicleModel_Code);

            var dto = new VehicleDTO() {
                VIN = Util.RandomString(EntityFieldLen.Vehicle_VIN).ToUpper(),
                ModelCode = vehicleModel.Code,
                LotNo = Util.RandomString(EntityFieldLen.Vehicle_LotNo).ToUpper(),
                KitNo = Util.RandomString(EntityFieldLen.Vehicle_KitNo).ToUpper(),
            };

            var before_VehicleCount = await ctx.Vehicles.CountAsync();

            // test
            var payload = await service.CreateVehicle(dto);
            
            // assert
            var errorCount = payload.Errors.Count();
            Assert.True(errorCount == 0, "Errors creating vehicle");
            
            var after_VehicleCount = await ctx.Vehicles.CountAsync();
            Assert.True(after_VehicleCount == before_VehicleCount + 1, "Vehicle count unchanged after save");

            var vehicle = await ctx.Vehicles.FirstOrDefaultAsync(t => t.VIN == dto.VIN);
            var vehicleComponentCount = vehicle.VehicleComponents.Count();
            var modelComponentCount = vehicleModel.ModelComponents.Count();

            Console.WriteLine($"**** vehicle {vehicleComponentCount} model {modelComponentCount} ****");
            Assert.True(modelComponentCount == vehicleComponentCount, "Vehicle components don't match vehicle model components");

            // lot no
            var vehicleLot = await ctx.VehicleLots.FirstOrDefaultAsync(t => t.LotNo == vehicle.LotNo);
            Assert.NotNull(vehicleLot);
        }

        [Fact]
        public async Task can_add_two_vehicles_to_same_lot() {
            var vehicleModel = await ctx.VehicleModels
                .FirstOrDefaultAsync(t => t.Code == TestVehicleModel_Code);

            var vehicle_lot_mo = Util.RandomString(EntityFieldLen.Vehicle_LotNo).ToUpper();
            
            var dto_1 = new VehicleDTO() {
                VIN = Util.RandomString(EntityFieldLen.Vehicle_VIN).ToUpper(),
                ModelCode = vehicleModel.Code,
                LotNo = vehicle_lot_mo,
                KitNo = vehicle_lot_mo + "01"
            };
            var dto_2 = new VehicleDTO() {
                VIN = Util.RandomString(EntityFieldLen.Vehicle_VIN).ToUpper(),
                ModelCode = vehicleModel.Code,
                LotNo = vehicle_lot_mo,
                KitNo = vehicle_lot_mo + "01"
            };

            var service = new VehicleService(ctx);

            var payload_1 = await service.CreateVehicle(dto_1);
            var payload_2 = await service.CreateVehicle(dto_2);

            Assert.True(payload_1.Errors.Count() == 0,"error creating vehicle 1");
            Assert.True(payload_2.Errors.Count() == 0,"error creating vehicle 2");

            var vehicleLot = await ctx.VehicleLots.FirstOrDefaultAsync(t => t.LotNo == vehicle_lot_mo);
            Assert.NotNull(vehicleLot);

            var vehicle_count = await ctx.Vehicles.CountAsync(t => t.LotId == vehicleLot.Id);
            Assert.Equal(2, vehicle_count);
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
