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
            // GenerateSeedData();
        }

        [Fact]
        public async Task can_create_vehicle_lot() {
            var modelCode = Util.RandomString(EntityFieldLen.VehicleModel_Code);
            // setup
            var model =  Gen_VehicleModel(ctx, modelCode, new List<(string, string)> {
                ("component_1", "station_1"),
                ("component_2", "station_2")
            });
            var lotNo = Util.RandomString(EntityFieldLen.Vehicle_LotNo).ToUpper();

            var kitVins = new List<(string, string)> {
                (Gen_KitNo(), Gen_Vin()),
                (Gen_KitNo(), Gen_Vin())
            };
 
            var vehicleLotDTO = Gen_VehicleLot_DTO(lotNo: lotNo, modelCode: modelCode, kitVins);

            // test
            var service = new VehicleService(ctx);
            var paylaod = await service.CreateVhicleLot(vehicleLotDTO);

            // assert
            Assert.True(0 == paylaod.Errors.Count(), "should not have errors");
            var vehicleLot = await ctx.VehicleLots.FirstOrDefaultAsync(t => t.LotNo == lotNo);
            Assert.NotNull(vehicleLot);
        }

        [Fact]
        public async Task cannot_create_vehicle_lot_with_no_vehicles() {
            // setup
            var modelCode = await ctx.VehicleModels.Select(t => t.Code).FirstOrDefaultAsync();
            var lotNo = Util.RandomString(EntityFieldLen.Vehicle_LotNo).ToUpper();

            var vin_num = Util.RandomString(EntityFieldLen.Vehicle_VIN).ToUpper();
            var vin_numbers = new string[] { };
            var vehicleLotDTO = Gen_VehicleLot_DTO(lotNo: lotNo, modelCode: modelCode, 
            new List<(string, string)> {
            });

            // test
            var service = new VehicleService(ctx);
            var payload = await service.CreateVhicleLot(vehicleLotDTO);

            // assert
            Assert.True(1 == payload.Errors.Count());
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            var expected = "no vehicles found in lot";
            var actual = errorMessage.Substring(0, expected.Length);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task cannot_create_vehicle_lot_with_duplicate_vins() {
            // setup
            var modelCode = await ctx.VehicleModels.Select(t => t.Code).FirstOrDefaultAsync();
            var lotNo = Util.RandomString(EntityFieldLen.Vehicle_LotNo).ToUpper();

            var vin_num = Util.RandomString(EntityFieldLen.Vehicle_VIN).ToUpper();
            var vehicleLotDTO = Gen_VehicleLot_DTO(lotNo: lotNo, modelCode: modelCode, 
            new List<(string, string)> {
                 ( Util.RandomString(EntityFieldLen.Vehicle_KitNo), vin_num ),
                 ( Util.RandomString(EntityFieldLen.Vehicle_KitNo), vin_num)
            });

            // test
            var service = new VehicleService(ctx);
            var payload = await service.CreateVhicleLot(vehicleLotDTO);

            // assert
            Assert.True(1 == payload.Errors.Count());
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            var expected = "duplicate vin in vehicle lot";
            var actual = errorMessage.Substring(0, expected.Length);
            Assert.Equal(expected, actual);
        }


        [Fact]
        public async Task cannot_create_vehicle_lot_if_model_code_does_not_exists() {
            // setup
            var modelCode = await ctx.VehicleModels.Select(t => t.Code).FirstOrDefaultAsync();
            var lotNo = Util.RandomString(EntityFieldLen.Vehicle_LotNo).ToUpper();

            var vin_num = Util.RandomString(EntityFieldLen.Vehicle_VIN).ToUpper();

            var nonExistendModelCode = Util.RandomString(EntityFieldLen.VehicleModel_Code);
            var vehicleLotDTO = Gen_VehicleLot_DTO(lotNo: lotNo, modelCode: modelCode, 
                new List<(string, string)> {
                    ( Util.RandomString(EntityFieldLen.Vehicle_KitNo),Util.RandomString(EntityFieldLen.Vehicle_VIN) )
                });

            // test
            var service = new VehicleService(ctx);
            var payload = await service.CreateVhicleLot(vehicleLotDTO);

            // assert
            Assert.True(1 == payload.Errors.Count());
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            var expected = "vehicle model not found";
            var actual = errorMessage.Substring(0, expected.Length);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task create_vehicle_duplication_vehicle_lot() {
            // setup
            var modelCode = Util.RandomString(EntityFieldLen.VehicleModel_Code);
            var model = Gen_VehicleModel(ctx, modelCode, new List<(string,string)> {
                ("component_1", "station_1")
            });
            var lotNo = Util.RandomString(EntityFieldLen.Vehicle_LotNo).ToUpper();

            // test
            var service = new VehicleService(ctx);

            var kitVins = new List<(string kitNo, string vin)> {
                (Gen_KitNo(), Gen_Vin())
            };

            var dto = Gen_VehicleLot_DTO(lotNo: lotNo, modelCode: modelCode, kitVins);
            var payload = await service.CreateVhicleLot(dto);
            Assert.True(0 == payload.Errors.Count());

            var payload_2 = await service.CreateVhicleLot(dto);
            Assert.True(1 == payload_2.Errors.Count());

            // assert
            var message = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
            Assert.Equal("duplicate vehicle lot", message);
        }

        private VehicleLotDTO Gen_VehicleLot_DTO(
            string lotNo, 
            string modelCode,  
            List<(string kitNo, string vin)> kitVins) {
            return new VehicleLotDTO {
                LotNo = lotNo,
                VehicleDTOs = kitVins.Select(kitVin => new VehicleDTO {
                    VIN = kitVin.vin,
                    KitNo = kitVin.kitNo,
                    ModelCode = modelCode,
                    LotNo = lotNo,
                }).ToList()
            };
        }
    }
}
