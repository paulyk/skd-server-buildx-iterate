using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class BomService_Test : TestBase {

        public BomService_Test() {
            ctx = GetAppDbContext();
            Gen_Baseline_Test_Seed_Data();
        }

        [Fact]
        private async Task can_import_vehicle_lot_parts() {
            // setup
            var plant = Gen_Plant();
            var lot1 = Gen_LotNo();
            var lot2 = Gen_LotNo();

            var dto = new BomLotPartsInput() {
                Sequence = 1,
                PlantCode = plant.Code,
                LotParts = new List<LotPartInput> {
                    new LotPartInput {
                        LotNo = lot1,
                        PartNo = "0001",
                        PartDesc = "part 1",
                        Quantity = 1
                    },
                    new LotPartInput {
                        LotNo = lot1,
                        PartNo = "0002",
                        PartDesc = "part 2",
                        Quantity = 1
                    },
                    new LotPartInput {
                        LotNo = lot2,
                        PartNo = "0001",
                        PartDesc = "part 1",
                        Quantity = 1
                    }
                }
            };

            // test
            var service = new BomService(ctx);
            var payload = await service.ImportBomLotParts(dto);

            // assert
            Assert.Equal(2, payload.Entity.LotCount);
            Assert.Equal(3, payload.Entity.LotPartCount);

            var after_count = ctx.LotParts.Count();
            Assert.Equal(3, after_count);
        }

        [Fact]
        private async Task cannot_import_duplicate_vehicle_lot_parts() {
            // setup
            var plant = Gen_Plant();
            var lotNo = Gen_LotNo();

            var dto = new BomLotPartsInput() {
                Sequence = 1,
                PlantCode = plant.Code,
                LotParts = new List<LotPartInput> {
                    new LotPartInput {
                        LotNo = lotNo,
                        PartNo = "0001",
                        PartDesc = "part 1",
                        Quantity = 1
                    },
                    new LotPartInput {
                        LotNo = lotNo,
                        PartNo = "0001",
                        PartDesc = "part 1",
                        Quantity = 3
                    }
                }
            };

            var before_count = ctx.LotParts.Count();
            // test
            var service = new BomService(ctx);
            var payload = await service.ImportBomLotParts(dto);

            // assert
            var expectedError = "duplicate Lot + Part number(s)";
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            errorMessage = (errorMessage ?? "").Substring(0, expectedError.Length);
            Assert.Equal(expectedError, errorMessage);
        }

        [Fact]
        private async Task cannot_import_if_no_vehicle_lot_parts() {
            // setup
            var plant = Gen_Plant();
            var dto = new BomLotPartsInput() {
                PlantCode = plant.Code,
                Sequence = 1,
                LotParts = new List<LotPartInput>()
            };

            var before_count = ctx.LotParts.Count();
            // test
            var service = new BomService(ctx);
            var payload = await service.ImportBomLotParts(dto);

            // assert
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            var expectedError = "no lot parts found";
            Assert.Equal(expectedError, errorMessage);
        }

        [Fact]
        private async Task can_import_vehicle_lots_from_bom() {

            // setup
            var plant = Gen_Plant();
            var model = await ctx.VehicleModels.FirstOrDefaultAsync();
            var lotNo = Gen_LotNo();
            var kitCount = 6;

            var input = Gen_BomLotKitInput(plant.Code, lotNo, model.Code, kitCount);

            // test
            var service = new BomService(ctx);
            var payload = await service.ImportBomLotKits(input);

            var bom = await ctx.Boms
                .Include(t => t.Lots).ThenInclude(t => t.Vehicles)
                .FirstOrDefaultAsync(t => t.Plant.Code == plant.Code);

            var lotCount = bom.Lots.Count();
            Assert.Equal(1, lotCount);
            var actualKitCount = bom.Lots.Sum(t => t.Vehicles.Count());
            Assert.Equal(kitCount, actualKitCount);
        }

        [Fact]
        private async Task cannot_import_lot_kits_from_bom_if_model_missing() {
            // setup
            var plant = Gen_Plant();
            var modelCode = Gen_VehicleModel_Code();
            var lotNo = Gen_LotNo();
            var kitCount = 6;

            var input = Gen_BomLotKitInput(plant.Code, lotNo, modelCode, kitCount);

            // test
            var service = new BomService(ctx);
            var payload = await service.ImportBomLotKits(input);

            var errorCount = payload.Errors.Count();
            Assert.Equal(1, errorCount);

            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            var expectedErrorMessage = "model codes not in system";
            Assert.Equal(expectedErrorMessage, errorMessage.Substring(0,expectedErrorMessage.Length ));
        }

        private BomLotKitInput Gen_BomLotKitInput(string plantCode, string lotNo, string modelCode, int kitCount = 6) {                    
            return new BomLotKitInput() {
                PlantCode = plantCode,
                Sequence = 1,
                Lots = new List<BomLotKitInput.Lot> {
                    new BomLotKitInput.Lot {
                        LotNo = Gen_LotNo(),
                        Kits = Enumerable.Range(1,kitCount).Select(num => new BomLotKitInput.Lot.Kit {
                            KitNo = Gen_KitNo(lotNo, num),
                            ModelCode = modelCode
                        }).ToList()
                    }
                }
            };
        }

    }
}
