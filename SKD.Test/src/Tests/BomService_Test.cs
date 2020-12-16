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
        }

        [Fact]
        private async Task can_import_bom_lot_parts() {
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
        private async Task cannot_import_duplicate_lot_parts() {
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
        private async Task cannot_import_if_no_lot_parts() {
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
    
        
        private void can_import_lot_kits_from_bom() {

        }

        private void cannot_import_lot_kits_from_bom_if_models_missing() {
            
        }

        private void cannot_import_lot_kits_from_bom_if_duplicate_kit_no() {
            
        }

    }
}
