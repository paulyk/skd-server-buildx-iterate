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

            var vehicleModel = await ctx.VehicleModels.FirstOrDefaultAsync();

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

            Assert.True(modelComponentCount == vehicleComponentCount, "Vehicle components don't match vehicle model components");

            // lot no
            var vehicleLot = await ctx.VehicleLots.FirstOrDefaultAsync(t => t.LotNo == vehicle.LotNo);
            Assert.NotNull(vehicleLot);
        }

        [Fact]
        public async Task can_add_two_vehicles_to_same_lot() {
            var vehicleModel = await ctx.VehicleModels.FirstOrDefaultAsync();

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

            Assert.True(payload_1.Errors.Count() == 0, "error creating vehicle 1");
            Assert.True(payload_2.Errors.Count() == 0, "error creating vehicle 2");

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

        [Fact]
        public async Task can_create_vehicle_lot() {
            // setup
            var modelCode = await ctx.VehicleModels.Select(t => t.Code).FirstOrDefaultAsync();
            var lotNo = Util.RandomString(EntityFieldLen.Vehicle_LotNo).ToUpper();

            var vin_numbers = new string[] { 
                Util.RandomString(EntityFieldLen.Vehicle_VIN).ToUpper(),
                Util.RandomString(EntityFieldLen.Vehicle_VIN).ToUpper(),
            };
            var vehicleLotDTO = GetNew_VehicleLotDTO(lotNo: lotNo, modelCode: modelCode, vin_numbers);

            // test
            var service = new VehicleService(ctx);
            var paylaod = await service.CreateVhicleLot(vehicleLotDTO);

            // assert
            Assert.True(0 == paylaod.Errors.Count(), "should not have errors");
            var vehicleLot = await ctx.VehicleLots.FirstOrDefaultAsync(t => t.LotNo == lotNo);
            Assert.NotNull(vehicleLot);
        }

        [Fact]
        public async Task cannot_create_vehicle_lot_with_duplicate_vins() {
            // setup
            var modelCode = await ctx.VehicleModels.Select(t => t.Code).FirstOrDefaultAsync();
            var lotNo = Util.RandomString(EntityFieldLen.Vehicle_LotNo).ToUpper();

            var vin_num = Util.RandomString(EntityFieldLen.Vehicle_VIN).ToUpper();
            var vin_numbers = new string[] { 
                Util.RandomString(EntityFieldLen.Vehicle_VIN).ToUpper(),
                vin_num,
                vin_num
            };
            var vehicleLotDTO = GetNew_VehicleLotDTO(lotNo: lotNo, modelCode: modelCode, vin_numbers);

            // test
            var service = new VehicleService(ctx);
            var payload = await service.CreateVhicleLot(vehicleLotDTO);

            // assert
            Assert.True(1 == payload.Errors.Count());
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            Console.WriteLine(errorMessage);
            Assert.True(errorMessage.StartsWith("duplicate vin in vehicle lot"));
        }

        [Fact]
        public async Task create_vehicle_lot_error_if_vehicle_lot_already_has_vehicles() {
            // setup
            var modelCode = await ctx.VehicleModels.Select(t => t.Code).FirstOrDefaultAsync();
            var lotNo = Util.RandomString(EntityFieldLen.Vehicle_LotNo).ToUpper();

            // test
            var service = new VehicleService(ctx);

            var vin_numbers = new string[] { 
                Util.RandomString(EntityFieldLen.Vehicle_VIN).ToUpper(),
                Util.RandomString(EntityFieldLen.Vehicle_VIN).ToUpper(),
            };

            var vehicleLotDTO_1 = GetNew_VehicleLotDTO(lotNo: lotNo, modelCode: modelCode, vin_numbers);
            var paylaod_1 = await service.CreateVhicleLot(vehicleLotDTO_1);

            var vehicleLotDTO_2 = GetNew_VehicleLotDTO(lotNo: lotNo, modelCode: modelCode, vin_numbers);
            var paylaod_2 = await service.CreateVhicleLot(vehicleLotDTO_1);

            // assert
            Assert.True(0 == paylaod_1.Errors.Count());
            Assert.True(1 == paylaod_2.Errors.Count());
            var message = paylaod_2.Errors.Select(t => t.Message).FirstOrDefault();
            Assert.Equal("duplicate vehicle lot", message);
        }

        private VehicleLotDTO GetNew_VehicleLotDTO(string lotNo, string modelCode,params string[] vins) {
            return new VehicleLotDTO {
                LotNo = lotNo,
                VehicleDTOs = vins.Select(vin => new VehicleDTO {
                    VIN = vin,
                    ModelCode = modelCode,
                    LotNo = lotNo,
                    KitNo = Util.RandomString(EntityFieldLen.Vehicle_KitNo)
                }).ToList()
            };            
        }

        private void GenerateSeedData() {

            var productionStations = Gen_ProductionStations(ctx, "station_1", "station_2");
            var components = Gen_Components(ctx, "component_1", "component_2");

            var vehicleModel_1 = new VehicleModel() {
                Code = Util.RandomString(EntityFieldLen.VehicleModel_Code).ToUpper(),
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
