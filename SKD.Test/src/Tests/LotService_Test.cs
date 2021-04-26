using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class LotService_Test : TestBase {

        public LotService_Test() {
            context = GetAppDbContext();
            Gen_Baseline_Test_Seed_Data(generateLot: false);
        }

        [Fact]
        private async Task import_lot_parts_works() {
            // setup
            var plant = Gen_Plant();
            var modelCode = await context.VehicleModels.Select(t => t.Code).FirstOrDefaultAsync();
            var lot1 = Gen_LotNo(modelCode, 1);
            var lot2 = Gen_LotNo(modelCode, 2);
            var bomFileSequnce = 1;
            var initial_lot_count = await context.Lots.CountAsync();

            var service = new LotService(context);

            // test
            var input = GenBomLotPartInput(new TestBomLotInput(plant.Code, bomFileSequnce, new List<TestLotInput> {
                new TestLotInput(lot1, new List<TestLotPartInput>{
                    new TestLotPartInput("part-1", 2),
                    new TestLotPartInput("part-2", 4),
                }),
                new TestLotInput(lot2, new List<TestLotPartInput>{
                    new TestLotPartInput("part-1", 3),
                    new TestLotPartInput("part-2", 4),
                }),
            }));

            // test
            await service.ImportBomLotParts(input);

            // assert
            var expected_lot_count = input.LotParts.Select(t => t.LotNo).Distinct().Count();
            var actual_lot_count = await context.Lots.CountAsync();
            Assert.Equal(initial_lot_count + expected_lot_count, actual_lot_count);

            // assert lot part count
            var exptected_part_count = input.LotParts.Select(t => t.PartNo).Distinct().Count();
            var actual_part_count = await context.Parts.CountAsync();
            Assert.Equal(exptected_part_count, actual_part_count);

            // assert lot part quantity
            var lotNos = input.LotParts.Select(t => t.LotNo).Distinct().ToList();
            var lotParts = await context.LotParts.Include(t => t.Part).Where(t => lotNos.Any(lotNo => lotNo == t.Lot.LotNo)).ToListAsync();
            input.LotParts.ToList().ForEach(inputLotPart => {
                LotPart lotPart = lotParts.First(t => t.Lot.LotNo == inputLotPart.LotNo && t.Part.PartNo == inputLotPart.PartNo);
                Assert.Equal(inputLotPart.Quantity, lotPart.BomQuantity);
            });
        }


        [Fact]
        private async Task multiple_bom_imports_update_lots_correctly() {
            // setup
            var plant = Gen_Plant();
            var modelCode = await context.VehicleModels.Select(t => t.Code).FirstOrDefaultAsync();
            var lot1 = Gen_LotNo(modelCode, 1);
            var lot2 = Gen_LotNo(modelCode, 2);
            var lot3 = Gen_LotNo(modelCode, 3);
            var bomFileSequnce = 1;
            var initial_lot_count = await context.Lots.CountAsync();

            var service = new LotService(context);

            // Initial 2 lots 2 parts
            var input_1 = GenBomLotPartInput(new TestBomLotInput(plant.Code, bomFileSequnce, new List<TestLotInput> {
                new TestLotInput(lot1, new List<TestLotPartInput>{
                    new TestLotPartInput("part-1", 2),
                    new TestLotPartInput("part-2", 4),
                }),
                new TestLotInput(lot2, new List<TestLotPartInput>{
                    new TestLotPartInput("part-1", 3),
                    new TestLotPartInput("part-2", 4),
                }),
            }));
            await service.ImportBomLotParts(input_1);
            var lot_count = await context.Lots.CountAsync();
            var exptected_lot_count = initial_lot_count + 2;
            Assert.Equal(exptected_lot_count, lot_count);

            // add new lot, with 2 new parts
            var input_2 = GenBomLotPartInput(new TestBomLotInput(plant.Code, bomFileSequnce, new List<TestLotInput> {
                new TestLotInput(lot1, new List<TestLotPartInput>{
                    new TestLotPartInput("part-1", 2),
                    new TestLotPartInput("part-2", 4),
                }),
                new TestLotInput(lot2, new List<TestLotPartInput>{
                    new TestLotPartInput("part-1", 3),
                    new TestLotPartInput("part-2", 4),
                }),
                new TestLotInput(lot3, new List<TestLotPartInput>{
                    new TestLotPartInput("part-3", 3),
                    new TestLotPartInput("part-4", 4),
                }),
            }));
            await service.ImportBomLotParts(input_2);

            lot_count = await context.Lots.CountAsync();
            exptected_lot_count = initial_lot_count + input_2.LotParts.Select(t => t.LotNo).Distinct().Count();
            Assert.Equal(exptected_lot_count, lot_count);

            // update quantity on lot1 part-2
            var lot1_part_2_quanity = input_1.LotParts.First(t => t.LotNo == lot1 && t.PartNo == "part-2").Quantity;
            var lot1_part_2_new_quanity = 20;

            var input_3 = GenBomLotPartInput(new TestBomLotInput(plant.Code, bomFileSequnce, new List<TestLotInput> {
                new TestLotInput(lot1, new List<TestLotPartInput>{
                    new TestLotPartInput("part-1", 2),
                    new TestLotPartInput("part-2", lot1_part_2_new_quanity),
                }),
                new TestLotInput(lot2, new List<TestLotPartInput>{
                    new TestLotPartInput("part-1", 3),
                    new TestLotPartInput("part-2", 4),
                }),
                new TestLotInput(lot3, new List<TestLotPartInput>{
                    new TestLotPartInput("part-3", 3),
                    new TestLotPartInput("part-4", 4),
                }),
            }));
            await service.ImportBomLotParts(input_3);

            var lotParts = await context.LotParts.Include(t => t.Lot).Include(t => t.Part).ToListAsync();
            foreach (var lotPart in lotParts.Where(t => t.RemovedAt == null)) {
                var inputLotPart = input_3.LotParts
                    .First(t => t.LotNo == lotPart.Lot.LotNo && t.PartNo == lotPart.Part.PartNo);
                // quantities eupdate
                Assert.Equal(inputLotPart.Quantity, lotPart.BomQuantity);
            }

            var replacedLotPart = lotParts.Where(t => t.RemovedAt != null).First();
            Assert.Equal(lot1_part_2_quanity, replacedLotPart.BomQuantity);

            // remove second part from every lot
            var input_4 = GenBomLotPartInput(new TestBomLotInput(plant.Code, bomFileSequnce, new List<TestLotInput> {
                new TestLotInput(lot1, new List<TestLotPartInput>{
                    new TestLotPartInput("part-1", 2),
                }),
                new TestLotInput(lot2, new List<TestLotPartInput>{
                    new TestLotPartInput("part-1", 3),
                }),
                new TestLotInput(lot3, new List<TestLotPartInput>{
                    new TestLotPartInput("part-3", 3),
                }),
            }));
            await service.ImportBomLotParts(input_4);

            var exptected_lot_part_count = input_4.LotParts.Count();
            var active_lot_part_count = await context.LotParts.Where(t => t.RemovedAt == null).CountAsync();
            Assert.Equal(exptected_lot_part_count, active_lot_part_count);

        }



        [Fact]
        private async Task import_bom_lot_part_will_remove_omitted_part() {
            // setup
            var plant = Gen_Plant();
            var modelCode = await context.VehicleModels.Select(t => t.Code).FirstOrDefaultAsync();
            var lot1 = Gen_LotNo(modelCode, 1);
            var bomFileSequnce = 1;

            var service = new LotService(context);

            // import lot1 
            var input_1 = GenBomLotPartInput(new TestBomLotInput(plant.Code, bomFileSequnce, new List<TestLotInput> {
                new TestLotInput(lot1, new List<TestLotPartInput>{
                    new TestLotPartInput("part-1", 2),
                    new TestLotPartInput("part-2", 3),
                })
            }));
            await service.ImportBomLotParts(input_1);

            // remove lot1 part-2 by excluding from input
            var input_2 = GenBomLotPartInput(new TestBomLotInput(plant.Code, ++bomFileSequnce, new List<TestLotInput> {
                new TestLotInput(lot1, new List<TestLotPartInput>{
                    new TestLotPartInput("part-1", 2),
                }),
            }));

            await service.ImportBomLotParts(input_2);
            var lotParts = await context.LotParts.Include(t => t.Part).Where(t => t.Lot.LotNo == lot1).ToListAsync();
            var active_lot_parts = lotParts.Where(t => t.RemovedAt == null).ToList();
            var removed_lot_parts = lotParts.Where(t => t.RemovedAt != null).ToList();

            Assert.True(1 == active_lot_parts.Count());
            Assert.True(1 == removed_lot_parts.Count());

            Assert.True("part-2" == removed_lot_parts.First().Part.PartNo);
        }

        [Fact]
        private async Task import_bom_lot_changes_part_no_format() {
            // setup
            var plant = Gen_Plant();
            var lot1 = Gen_LotNo(1);
            var lot2 = Gen_LotNo(2);

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
            var service = new LotService(context);
            var payload = await service.ImportBomLotParts(input);

            var partService = new PartService(context);

            // assert 
            foreach (var entry in part_mubers) {
                var part = await context.Parts.FirstOrDefaultAsync(t => t.PartNo == entry.reformattedPartNo);
                var formatted = PartService.ReFormatPartNo(part.PartNo);
                Assert.Equal(formatted, part.PartNo);
            }
        }



        [Fact]
        private async Task cannot_import_duplicate_bom_lot_parts_in_paylaod() {
            // setup
            var plant = Gen_Plant();
            var lotNo = Gen_LotNo(1);

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

            var before_count = context.LotParts.Count();
            // test
            var service = new LotService(context);
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

            var before_count = context.LotParts.Count();
            // test
            var service = new LotService(context);
            var payload = await service.ImportBomLotParts(dto);

            // assert
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            var expectedError = "no lot parts found";
            Assert.Equal(expectedError, errorMessage);
        }

        [Fact]
        private async Task can_import_bom_lot_kits() {

            // setup
            var plant = Gen_Plant();
            var model = await context.VehicleModels.FirstOrDefaultAsync();
            var lotNo = Gen_LotNo(1);
            var kitCount = 6;

            var input = Gen_BomLotKitInput(plant.Code, lotNo, model.Code, kitCount);

            // test
            var service = new LotService(context);
            var payload = await service.ImportBomLotKits(input);

            // assert
            var bom = await context.Boms
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
            var lotNo = Gen_LotNo(1);
            var kitCount = 6;

            var input = Gen_BomLotKitInput(plant.Code, lotNo, modelCode, kitCount);

            // test
            var service = new LotService(context);
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
            var lotNo = Gen_LotNo(1);
            var model = await context.VehicleModels.FirstOrDefaultAsync();
            var kitCount = 6;

            var input = Gen_BomLotKitInput(plant.Code, lotNo, model.Code, kitCount);

            // test
            var service = new LotService(context);
            var payload = await service.ImportBomLotKits(input);
            var payload_2 = await service.ImportBomLotKits(input);

            // assert
            var errorMessage = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
            var expectedMessage = "kit numbers already imported";
            Assert.Equal(expectedMessage, errorMessage);
        }

        [Fact]
        private async Task can_set_lot_note() {
            Gen_Bom_Lot_and_Kits();
            // setup
            var lot = await context.Lots.FirstOrDefaultAsync();

            var service = new LotService(context);
            
            //
            var note = "The note";
            var input = new LotNoteInput(lot.LotNo, note);
            var paylaod = await service.SetLotNote(input);

            var updatedLot = await context.Lots.FirstOrDefaultAsync(t => t.LotNo == input.LotNo);

            Assert.Equal(input.Note, updatedLot.Note);
        }

        #region helpers

        private BomLotKitInput Gen_BomLotKitInput(string plantCode, string lotNo, string modelCode, int kitCount = 6) {
            return new BomLotKitInput() {
                PlantCode = plantCode,
                Sequence = 1,
                Lots = new List<BomLotKitInput.Lot> {
                    new BomLotKitInput.Lot {
                        LotNo = Gen_LotNo(modelCode, 1),
                        Kits = Enumerable.Range(1,kitCount).Select(num => new BomLotKitInput.Lot.LotKit {
                            KitNo = Gen_KitNo(lotNo, num),
                            ModelCode = modelCode
                        }).ToList()
                    }
                }
            };
        }
        private record TestLotPartInput(string PartNo, int Quantity);
        private record TestLotInput(string LotNo, IEnumerable<TestLotPartInput> LotParts);
        private record TestBomLotInput(string PlantCode, int BomFileSequence, IEnumerable<TestLotInput> Lots);
        private BomLotPartInput GenBomLotPartInput(TestBomLotInput input)
            => new BomLotPartInput {
                PlantCode = input.PlantCode,
                Sequence = input.BomFileSequence,
                LotParts = input.Lots.SelectMany(l => l.LotParts.Select(lp => new BomLotPartInput.LotPart {
                    LotNo = l.LotNo,
                    PartNo = lp.PartNo,
                    PartDesc = lp.PartNo + "-desc",
                    Quantity = lp.Quantity
                })).ToList()
            };


        #endregion

    }
}
