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

            var dto = new BomLotPartInput() {
                Sequence = 1,
                PlantCode = plant.Code,
                LotParts = new List<BomLotPartInput.LotPart> {
                    new BomLotPartInput.LotPart {
                        LotNo = lot1,
                        PartNo = "0001",
                        PartDesc = "part 1",
                        Quantity = 1
                    },
                    new BomLotPartInput.LotPart {
                        LotNo = lot1,
                        PartNo = "0002",
                        PartDesc = "part 2",
                        Quantity = 1
                    },
                    new BomLotPartInput.LotPart {
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
            Assert.Equal(2, payload.Entity.PartCount);

            var after_count = ctx.LotParts.Count();
            Assert.Equal(3, after_count);
        }

        [Fact]
        private async Task cannot_import_duplicate_bom_lot_parts_in_paylaod() {
            // setup
            var plant = Gen_Plant();
            var lotNo = Gen_LotNo();

            var dto = new BomLotPartInput() {
                Sequence = 1,
                PlantCode = plant.Code,
                LotParts = new List<BomLotPartInput.LotPart> {
                    new BomLotPartInput.LotPart {
                        LotNo = lotNo,
                        PartNo = "0001",
                        PartDesc = "part 1",
                        Quantity = 1
                    },
                    new BomLotPartInput.LotPart {
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
            var expectedError = "duplicate Lot + Part number(s) in payload";
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            errorMessage = (errorMessage ?? "").Substring(0, expectedError.Length);
            Assert.Equal(expectedError, errorMessage);
        }

        [Fact]
        private async Task cannot_import_bom_lot_parts_if_already_imported() {
            // setup
            var plant = Gen_Plant();
            var lotNo = Gen_LotNo();

            var input = new BomLotPartInput() {
                Sequence = 1,
                PlantCode = plant.Code,
                LotParts = new List<BomLotPartInput.LotPart> {
                    new BomLotPartInput.LotPart {
                        LotNo = lotNo,
                        PartNo = "0001",
                        PartDesc = "part 1",
                        Quantity = 1
                    },
                    new BomLotPartInput.LotPart {
                        LotNo = lotNo,
                        PartNo = "0002",
                        PartDesc = "part 2jj",
                        Quantity = 3
                    }
                }
            };

            // test
            var service = new BomService(ctx);
            var payload = await service.ImportBomLotParts(input);
            var payload_2 = await service.ImportBomLotParts(input);

            // assert
            var errorMessage = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
            var expectedMessage = "lot parts already imported";
            Assert.Equal(expectedMessage, errorMessage);
        }


        [Fact]
        private async Task cannot_import_if_no_vehicle_lot_parts() {
            // setup
            var plant = Gen_Plant();
            var dto = new BomLotPartInput() {
                PlantCode = plant.Code,
                Sequence = 1,
                LotParts = new List<BomLotPartInput.LotPart>()
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
        private async Task can_import_bom_lot_kits_from_bom() {

            // setup
            var plant = Gen_Plant();
            var model = await ctx.VehicleModels.FirstOrDefaultAsync();
            var lotNo = Gen_LotNo();
            var kitCount = 6;

            var input = Gen_BomLotKitInput(plant.Code, lotNo, model.Code, kitCount);

            // test
            var service = new BomService(ctx);
            var payload = await service.ImportBomLotKits(input);

            // assert
            var bom = await ctx.Boms
                .Include(t => t.Lots).ThenInclude(t => t.Vehicles)
                .FirstOrDefaultAsync(t => t.Plant.Code == plant.Code);

            var lotCount = bom.Lots.Count();
            Assert.Equal(1, lotCount);
            var actualKitCount = bom.Lots.Sum(t => t.Vehicles.Count());
            Assert.Equal(kitCount, actualKitCount);
        }

        [Fact]
        private async Task cannot_import_bom_lot_kits_if_model_missing() {
            // setup
            var plant = Gen_Plant();
            var modelCode = Gen_VehicleModel_Code();
            var lotNo = Gen_LotNo();
            var kitCount = 6;

            var input = Gen_BomLotKitInput(plant.Code, lotNo, modelCode, kitCount);

            // test
            var service = new BomService(ctx);
            var payload = await service.ImportBomLotKits(input);

            // assert
            var errorCount = payload.Errors.Count();
            Assert.Equal(1, errorCount);

            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            var expectedErrorMessage = "model codes not in system";
            Assert.Equal(expectedErrorMessage, errorMessage.Substring(0, expectedErrorMessage.Length));
        }

        [Fact]
        private async Task cannot_import_bom_lot_kits_already_imported() {
            // setup
            var plant = Gen_Plant();
            var lotNo = Gen_LotNo();
            var model = await ctx.VehicleModels.FirstOrDefaultAsync();
            var kitCount = 6;

            var input = Gen_BomLotKitInput(plant.Code, lotNo, model.Code, kitCount);

            // test
            var service = new BomService(ctx);
            var payload = await service.ImportBomLotKits(input);
            var payload_2 = await service.ImportBomLotKits(input);

            // assert
            var errorMessage = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
            var expectedMessage = "kit numbers already imported";
            Assert.Equal(expectedMessage, errorMessage);

        }

        private BomLotKitInput Gen_BomLotKitInput(string plantCode, string lotNo, string modelCode, int kitCount = 6) {
            return new BomLotKitInput() {
                PlantCode = plantCode,
                Sequence = 1,
                Lots = new List<BomLotKitInput.Lot> {
                    new BomLotKitInput.Lot {
                        LotNo = Gen_LotNo(),
                        Kits = Enumerable.Range(1,kitCount).Select(num => new BomLotKitInput.Lot.LotKit {
                            KitNo = Gen_KitNo(lotNo, num),
                            ModelCode = modelCode
                        }).ToList()
                    }
                }
            };
        }

    }
}
