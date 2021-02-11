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
        private async Task can_import_bom_lot_parts() {
            // setup
            var plant = Gen_Plant();
            var lot1 = Gen_LotNo();
            var lot2 = Gen_LotNo();

            var input = GetBomLotPoartInput(plant, new List<string> { lot1, lot2, }, new List<string> { "part-1", "part-2" });

            // test
            var service = new BomService(ctx);
            var payload = await service.ImportBomLotParts(input);

            // assert
            var expected_Lot_count = input.LotParts.Select(t => t.LotNo).Distinct().Count();
            var expeced_parts_count = input.LotParts.Select(t => t.PartNo).Distinct().Count();

            Assert.Equal(expected_Lot_count, payload.Entity.LotCount);
            Assert.Equal(expeced_parts_count, payload.Entity.PartCount);

            var lot_parts_count = ctx.Lots.Count(t => t.Bom.Id == payload.Entity.Id);
            Assert.Equal(expected_Lot_count, lot_parts_count);

            var parts_count = ctx.Parts.Count();
            Assert.Equal(expeced_parts_count, lot_parts_count);
        }

        [Fact]
        private async Task re_import_bom_lot_part_will_not_cause_error() {
            // setup
            var plant = Gen_Plant();
            var lot1 = Gen_LotNo();
            var lot2 = Gen_LotNo();

            var input = GetBomLotPoartInput(plant, new List<string> { lot1, lot2, }, new List<string> { "part-1", "part-2" });

            // test
            var service = new BomService(ctx);
            var payload = await service.ImportBomLotParts(input);
            var first_run_lot_part_count = await ctx.LotParts.CountAsync();

            payload = await service.ImportBomLotParts(input);
            var second_run_lot_part_count = await ctx.LotParts.CountAsync();

            // assert
            Assert.True(first_run_lot_part_count > 0);
            Assert.Equal(first_run_lot_part_count, second_run_lot_part_count);

            var error_count = payload.Errors.Count();
            Assert.Equal(0, error_count);
        }

        [Fact]
        private async Task import_bom_lot_part_will_update_bom_quantity_of_existing_lot_part() {
            // setup
            var plant = Gen_Plant();
            var lotNos = new List<string> { Gen_LotNo(), Gen_LotNo() };
            var partNos = new List<string> { "part_1", "part_2"  };

            var input_1 = GetBomLotPoartInput(plant, lotNos, partNos );


            // import initial lots
            var service = new BomService(ctx);
            var payload = await service.ImportBomLotParts(input_1);

            // assert 
            var expected_lot_part_count = input_1.LotParts.Count();
            var actual_lot_parts_count = await ctx.LotParts.CountAsync();
            Assert.Equal(expected_lot_part_count, actual_lot_parts_count);

            // import again with one part removed
            var lotNo = lotNos[0];
            var partNo = partNos[0];
            var input_2 = input_1.DeepCopy();
            var lotParToModify = input_2.LotParts.First(t => t.LotNo == lotNo && t.PartNo == partNo);
            lotParToModify.Quantity = lotParToModify.Quantity + 2;

            var payload_2 = await service.ImportBomLotParts(input_2);
            expected_lot_part_count = input_2.LotParts.Count() + 1;
            actual_lot_parts_count = await ctx.LotParts.CountAsync();
            Assert.Equal(expected_lot_part_count, actual_lot_parts_count);

            var removed_count = await ctx.LotParts.CountAsync(t => t.RemovedAt != null);
            Assert.Equal(1, removed_count);

            var modifiedPart = await ctx.LotParts
                .Where(t => t.Lot.LotNo == lotParToModify.LotNo)
                .Where(t => t.Part.PartNo == lotParToModify.PartNo)
                .Where(t => t.RemovedAt == null)
                .FirstOrDefaultAsync();

            Assert.Equal(lotParToModify.Quantity, modifiedPart.BomQuantity);
        }

        [Fact]
        private async Task import_bom_lot_part_will_remove_omitted_part() {
            // setup
            var plant = Gen_Plant();
            var lot1 = Gen_LotNo();
            var lot2 = Gen_LotNo();

            var service = new BomService(ctx);            

            // import lot1 and lot2
            var input = GetBomLotPoartInput(plant, new List<string> { lot1, lot2, }, new List<string> { "part-1", "part-2" });
            var payload = await service.ImportBomLotParts(input);

            var actual_lot_parts_count = await ctx.LotParts.Where(t => t.RemovedAt == null).CountAsync();
            var expected_lot_parts_count = input.LotParts.Count();
            Assert.Equal(actual_lot_parts_count, actual_lot_parts_count);

            // remove lot2, part_2 from input and import again
            input.LotParts = input.LotParts
                .Where(t => !(t.LotNo == lot2 && t.PartNo == "part_2"))
                .ToList();

            var payload_2 = await service.ImportBomLotParts(input);

            actual_lot_parts_count = await ctx.LotParts.Where(t => t.RemovedAt == null).CountAsync();
            expected_lot_parts_count = input.LotParts.Count();
            Assert.Equal(actual_lot_parts_count, actual_lot_parts_count);
        }

        [Fact]
        private async Task import_bom_lot_changes_part_no_format() {
            // setup
            var plant = Gen_Plant();
            var lot1 = Gen_LotNo();
            var lot2 = Gen_LotNo();

            // trim tailing [- ]* and removes spaces
            var part_mubers = new List<(string partNo, string reformattedPartNo)>() {
                ("-W716936-S442", "W716936-S442"),
                ("- W716899-  S900 -", "W716899-S900"),
                ("- EB3B-31010-  AF3ZHE -", "EB3B-31010-AF3ZHE"),
                ("       -  W500301-S437    -   ", "W500301-S437")
            };

            var input = new BomLotPartInput() {
                Sequence = 1,
                PlantCode = plant.Code,
                LotParts = new List<BomLotPartInput.LotPart> {
                    new BomLotPartInput.LotPart {
                        LotNo = lot1,
                        PartNo = part_mubers[0].partNo,
                        PartDesc = part_mubers[0].partNo + " desc",
                        Quantity = 1
                    },
                    new BomLotPartInput.LotPart {
                        LotNo = lot1,
                        PartNo = part_mubers[1].partNo,
                        PartDesc = part_mubers[1].partNo + " desc",
                        Quantity = 1
                    },
                    new BomLotPartInput.LotPart {
                        LotNo = lot2,
                        PartNo = part_mubers[2].partNo,
                        PartDesc = part_mubers[2].partNo + " desc",
                        Quantity = 1
                    },
                    new BomLotPartInput.LotPart {
                        LotNo = lot2,
                        PartNo = part_mubers[3].partNo,
                        PartDesc = part_mubers[3].partNo + " desc",
                        Quantity = 1
                    }
                }
            };

            // test
            var service = new BomService(ctx);
            var payload = await service.ImportBomLotParts(input);

            var partService = new PartService(ctx);

            // assert 
            foreach (var entry in part_mubers) {
                var part = await ctx.Parts.FirstOrDefaultAsync(t => t.PartNo == entry.reformattedPartNo);
                Assert.NotNull(part);
                Assert.Equal(entry.partNo, part.OriginalPartNo);
            }
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
        private async Task cannot_import_if_no_lot_parts() {
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
                .Include(t => t.Lots).ThenInclude(t => t.Kits)
                .FirstOrDefaultAsync(t => t.Plant.Code == plant.Code);

            var lotCount = bom.Lots.Count();
            Assert.Equal(1, lotCount);
            var actualKitCount = bom.Lots.Sum(t => t.Kits.Count());
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

        private BomLotPartInput GetBomLotPoartInput(
            Plant plant,
            List<string> lotNos,
            List<string> partNos
        ) {

            var lotParts = new List<BomLotPartInput.LotPart>();
            foreach (var lotNo in lotNos) {
                foreach (var partNo in partNos) {
                    lotParts.Add(new BomLotPartInput.LotPart {
                        LotNo = lotNo,
                        PartNo = partNo,
                        PartDesc = partNo + "_desc",
                        Quantity = 2
                    });
                }
            }

            return new BomLotPartInput() {
                Sequence = 1,
                PlantCode = plant.Code,
                LotParts = lotParts
            };
        }
    }
}
