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
        }

        [Fact]
        public async Task can_create_vehicle_lot() {
            // setup
            var modelCode = Gen_VehicleModel_Code();
            var model = Gen_VehicleModel(ctx, modelCode, new List<(string, string)> {
                ("component_1", "station_1"),
                ("component_2", "station_2")
            });
            var lotNo = Gen_LotNo();
            var kitNos = new List<string> { Gen_KitNo(), Gen_KitNo() };
            var vehicleLotDTO = Gen_VehicleLot_DTO(lotNo, modelCode, kitNos);

            var before_count = await ctx.VehicleLots.CountAsync();
            // test
            var service = new VehicleService(ctx);
            var paylaod = await service.CreateVehicleLot(vehicleLotDTO);
            var after_count = await ctx.VehicleLots.CountAsync();

            // assert
            Assert.Equal(before_count + 1, after_count);
        }

        [Fact]
        public async Task cannot_create_vehicle_lot_without_vehicles() {
            // setup            
            var modelCode = Gen_VehicleModel_Code();
            Gen_VehicleModel(ctx, modelCode, new List<(string componentCode, string stationCode)>{
                ("component_1", "statin_1")
            });

            var lotNo = Util.RandomString(EntityFieldLen.Vehicle_LotNo).ToUpper();
            var kitNos = new List<string> { };
            var vehicleLotDTO = Gen_VehicleLot_DTO(lotNo: lotNo, modelCode: modelCode, kitNos);

            // test
            var service = new VehicleService(ctx);
            var payload = await service.CreateVehicleLot(vehicleLotDTO);

            // assert
            Assert.True(1 == payload.Errors.Count());
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            var expected = "no vehicles found in lot";
            var actual = errorMessage.Substring(0, expected.Length);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task cannot_create_vehicle_lot_with_duplicate_kitNos() {
            // setup
            var modelCode = Gen_VehicleModel_Code();
            var model = Gen_VehicleModel(ctx, modelCode, new List<(string, string)> {
                ("component_1", "station_1"),
                ("component_2", "station_2")
            });
            var lotNo = Util.RandomString(EntityFieldLen.Vehicle_LotNo).ToUpper();

            var kitNo = Gen_KitNo();
            var kitNos = new List<string> { kitNo, kitNo };
            var vehicleLotDTO = Gen_VehicleLot_DTO(lotNo: lotNo, modelCode: modelCode, kitNos);

            // test
            var service = new VehicleService(ctx);
            var payload = await service.CreateVehicleLot(vehicleLotDTO);

            // assert
            Assert.True(1 == payload.Errors.Count());
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            var expected = "duplicate kitNo in vehicle lot";
            var actual = errorMessage.Substring(0, expected.Length);
            Assert.Equal(expected, actual);
        }


        [Fact]
        public async Task cannot_create_vehicle_lot_if_model_code_does_not_exists() {
            // setup
            var lotNo = Util.RandomString(EntityFieldLen.Vehicle_LotNo).ToUpper();
            var kitNos = new List<string> { Gen_KitNo(), Gen_KitNo() };

            var nonExistendModelCode = Util.RandomString(EntityFieldLen.VehicleModel_Code);
            var vehicleLotDTO = Gen_VehicleLot_DTO(lotNo: lotNo, modelCode: nonExistendModelCode, kitNos);

            // test
            var service = new VehicleService(ctx);
            var payload = await service.CreateVehicleLot(vehicleLotDTO);

            // assert
            Assert.True(1 == payload.Errors.Count());
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            var expected = "vehicle model not found";
            var actual = errorMessage.Substring(0, expected.Length);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task cannot_create_duplicate_vehicle_lot() {
            // setup
            var modelCode = Gen_VehicleModel_Code();
            var model = Gen_VehicleModel(ctx, modelCode, new List<(string, string)> {
                ("component_1", "station_1")
            });
            var lotNo = Gen_LotNo();

            var kitNos = new List<string> { Gen_KitNo(), Gen_KitNo() };
            var dto = Gen_VehicleLot_DTO(lotNo: lotNo, modelCode: modelCode, kitNos);

            // test
            var service = new VehicleService(ctx);
            try {
                var payload = await service.CreateVehicleLot(dto);

                Assert.True(0 == payload.Errors.Count());

                var payload_2 = await service.CreateVehicleLot(dto);
                Assert.True(1 == payload_2.Errors.Count());

                // assert
                var message = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
                Assert.Equal("duplicate vehicle lot", message);
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                var inner = ex.InnerException;
                while (inner != null) {
                    Console.WriteLine(inner.Message); ;
                    inner = inner.InnerException;
                }
            }
        }

        [Fact]
        public async Task can_assing_vehicle_kit_vins() {
            // setup
            var lotNo = Gen_LotNo();
            var vehicleLot = await Gen_Vehicle_Lot(lotNo);

            var kitVinDto = new VehicleKitVinDTO {
                LotNo = lotNo,
                Kits = vehicleLot.Vehicles.Select(t => new KitVinDTO {
                    KitNo = t.KitNo,
                    VIN = Gen_Vin()
                }).ToList()
            };

            // test
            var service = new VehicleService(ctx);
            var payload_2 = await service.AssingVehicleKitVin(kitVinDto);

            // assert
            var errorCount_2 = payload_2.Errors.Count();
            Assert.Equal(0, errorCount_2);
        }


        [Fact]
        public async Task cannot_assing_vehicle_lot_vins_if_vins_already_assigned() {
            // setup
            var lotNo = Gen_LotNo();
            var vehicleLot = await Gen_Vehicle_Lot(lotNo);

            var kitVinDto = new VehicleKitVinDTO {
                LotNo = lotNo,
                Kits = vehicleLot.Vehicles.Select(t => new KitVinDTO {
                    KitNo = t.KitNo,
                    VIN = Gen_Vin()
                }).ToList()
            };

            // test
            var service = new VehicleService(ctx);
            var payload_2 = await service.AssingVehicleKitVin(kitVinDto);
            var payload_3 = await service.AssingVehicleKitVin(kitVinDto);

            // assert
            var errorMessage = payload_3.Errors.Select(t => t.Message).FirstOrDefault();
            var expectedError = "vehicle lot VINs already assigned";
            Assert.Equal(expectedError, errorMessage);
        }

        [Fact]
        public async Task cannot_assing_vehicle_lot_vins_kits_not_found() {
            // setup
            var lotNo = Gen_LotNo();
            var vehicleLot = await Gen_Vehicle_Lot(lotNo);

            var kitVinDto = new VehicleKitVinDTO {
                LotNo = lotNo,
                Kits = vehicleLot.Vehicles.Select(t => new KitVinDTO {
                    KitNo = Gen_KitNo(), // generate a kit not thats different
                    VIN = Gen_Vin()
                }).ToList()
            };

            // test
            var service = new VehicleService(ctx);
            var payload_2 = await service.AssingVehicleKitVin(kitVinDto);

            // assert
            var expectedError = "kit numbers not found in lot";
            var errorMessage = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
            errorMessage = errorMessage.Substring(0, expectedError.Length);
            Assert.Equal(expectedError, errorMessage);

        }

        [Fact]
        public async Task cannot_assing_vehicle_lot_with_duplicate_kits_in_payload() {
            // setup
            var lotNo = Gen_LotNo();
            var vehicleLot = await Gen_Vehicle_Lot(lotNo);

            var kitNo = Gen_KitNo();
            var kitVinDto = new VehicleKitVinDTO {
                LotNo = lotNo,
                Kits = vehicleLot.Vehicles.Select(t => new KitVinDTO {
                    KitNo = kitNo,
                    VIN = Gen_Vin()
                }).ToList()
            };

            // test
            var service = new VehicleService(ctx);
            var payload_2 = await service.AssingVehicleKitVin(kitVinDto);

            // assert
            var expectedError = "duplicate kitNo(s) in payload";
            var errorMessage = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
            errorMessage = errorMessage.Substring(0, expectedError.Length);
            Assert.Equal(expectedError, errorMessage);
        }
        
        
        [Fact]
        public async Task can_set_timline_custom_received_at() {
            // setup
            var vehicle = Gen_Vehicle_And_Model(
                ctx,
                vin: Gen_Vin(),
                kitNo: Gen_KitNo(),
                lotNo: Gen_LotNo(),
                modelCode: Gen_VehicleModel_Code(),
                new List<(string, string)> { 
                    ("component_1", "station_1")
                }
            );
            
            var customReleasedDate = new DateTime(2020, 11, 1);
            var dto = new VehicleTimelineDTO {
                DateType = TimelineDateType.CustomReceived,
                VIN = vehicle.VIN,
                Date = customReleasedDate,
            };

            // test
            var service = new VehicleService(ctx);
            var paylaod = await service.UpdateVehicleTimeline(dto);

            // assert
            var timeline = ctx.VehicleTimelines.FirstOrDefault(t => t.Vehicle.VIN == vehicle.VIN);
            Assert.NotNull(timeline);

            Assert.Equal(customReleasedDate, timeline.CustomReceivedAt);
            Assert.Null( timeline.PlanBuildAt);
            Assert.Null( timeline.BuildCompletedAt);
            Assert.Null( timeline.GateRleaseAt);
            Assert.Null( timeline.WholeStateAt);

            Assert.Equal(DateTime.UtcNow.Date, timeline.ModifiedAt.Date);
        }
        
        [Fact]
        public async Task can_set_timline_plan_build_at() {
            // setup
            var vehicle = Gen_Vehicle_And_Model(
                ctx,
                vin: Gen_Vin(),
                kitNo: Gen_KitNo(),
                lotNo: Gen_LotNo(),
                modelCode: Gen_VehicleModel_Code(),
                new List<(string, string)> { 
                    ("component_1", "station_1")
                }
            );
            
            var customReleasedDate = new DateTime(2020, 11, 1);
            var planBuidDate = customReleasedDate.AddDays(1);

            var dto_custom_received = new VehicleTimelineDTO {
                DateType = TimelineDateType.CustomReceived,
                VIN = vehicle.VIN,
                Date = customReleasedDate
            };

            var dto_custom_plan_build = new VehicleTimelineDTO {
                DateType = TimelineDateType.PlanBuild,
                VIN = vehicle.VIN,
                Date = customReleasedDate.AddDays(1)
            };

            // test
            var service = new VehicleService(ctx);
            var paylaod_1 = await service.UpdateVehicleTimeline(dto_custom_received);
            var paylaod_2 = await service.UpdateVehicleTimeline(dto_custom_plan_build);

            // assert
            var timeline = ctx.VehicleTimelines.FirstOrDefault(t => t.Vehicle.VIN == vehicle.VIN);
            Assert.NotNull(timeline);

            Assert.Equal(customReleasedDate, timeline.CustomReceivedAt);
            Assert.Equal(planBuidDate, timeline.PlanBuildAt);
            Assert.Null(timeline.BuildCompletedAt);
            Assert.Null( timeline.GateRleaseAt);
            Assert.Null( timeline.WholeStateAt);

            Assert.Equal(DateTime.UtcNow.Date, timeline.ModifiedAt.Date);
        }
        
        private async Task<VehicleLot> Gen_Vehicle_Lot(string lotNo, params string[] kitNos) {

            kitNos = kitNos.Length > 0 
                ? kitNos : 
                new string[] { Gen_KitNo(), Gen_KitNo() };

            var modelCode = Gen_VehicleModel_Code();
            var modelNo = Gen_VehicleModel(
                ctx,
                modelCode,
                component_stations_maps: new List<(string, string)> {
                    ("comp_1", "stat_1")
                });

            var vehicleLotDto = Gen_VehicleLot_DTO(
                lotNo,
                modelCode,
                kitNos.ToList()
             );

            var service = new VehicleService(ctx);
            var payload = await service.CreateVehicleLot(vehicleLotDto);
            var vehicleLot = await ctx.VehicleLots
                .Include(t => t.Vehicles)
                .FirstOrDefaultAsync(t => t.LotNo == lotNo);
            return vehicleLot;
        }

        private VehicleLotDTO Gen_VehicleLot_DTO(
            string lotNo,
            string modelCode,
            List<string> kitNos) {
            return new VehicleLotDTO {
                LotNo = lotNo,
                Kits = kitNos.Select(kitNo => new VehicleLotDTO.Kit {
                    KitNo = kitNo,
                    ModelCode = modelCode,
                }).ToList()
            };
        }
    }
}
