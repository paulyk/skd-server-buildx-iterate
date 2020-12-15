using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class BomSummaryService_Test : TestBase {

        public BomSummaryService_Test() {
            ctx = GetAppDbContext();
        }

        [Fact]
        private async Task can_create_bom_summary() {
            // setup
            var plant = Gen_Plant();
            var lot1 = Gen_LotNo();
            var lot2 = Gen_LotNo();

            var dto = new BomSummaryInput() {
                Sequence = 1,
                PlantCode = plant.Code,
                Parts = new List<BomSummaryPartInput> {
                    new BomSummaryPartInput {
                        LotNo = lot1,
                        PartNo = "0001",
                        PartDesc = "part 1",
                        Quantity = 1
                    },
                    new BomSummaryPartInput {
                        LotNo = lot1,
                        PartNo = "0002",
                        PartDesc = "part 2",
                        Quantity = 1
                    },
                    new BomSummaryPartInput {
                        LotNo = lot2,
                        PartNo = "0001",
                        PartDesc = "part 1",
                        Quantity = 1
                    }
                }
            };

            // test
            var bomService = new BomSummaryService(ctx);
            var payload = await bomService.CreateBomSummary(dto);

            // assert
            Assert.Equal(2, payload.Entity.LotCount);
            Assert.Equal(3, payload.Entity.LotPartCount);

            var after_count = ctx.BomSummaryParts.Count();
            Assert.Equal(3, after_count);
        }


        [Fact]
        private async Task cannot_create_bom_summary_with_duplicate_lot_and_part() {
            // setup
            var plant = Gen_Plant();
            var lotNo = Gen_LotNo();

            var dto = new BomSummaryInput() {
                Sequence = 1,
                PlantCode = plant.Code,
                Parts = new List<BomSummaryPartInput> {
                    new BomSummaryPartInput {
                        LotNo = lotNo,
                        PartNo = "0001",
                        PartDesc = "part 1",
                        Quantity = 1
                    },
                    new BomSummaryPartInput {
                        LotNo = lotNo,
                        PartNo = "0001",
                        PartDesc = "part 1",
                        Quantity = 3
                    }
                }
            };

            var before_count = ctx.BomSummaryParts.Count();
            // test
            var bomService = new BomSummaryService(ctx);
            var payload = await bomService.CreateBomSummary(dto);

            // assert
            var expectedError = "bom summary cannot have duplicate Lot + Part numbers";
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            errorMessage = (errorMessage ?? "").Substring(0, expectedError.Length);
            Assert.Equal(expectedError, errorMessage);
        }

        [Fact]
        private async Task cannot_create_bom_summary_with_no_pards() {
            // setup
            var plant = Gen_Plant();
            var dto = new BomSummaryInput() {
                PlantCode = plant.Code,
                Sequence = 1,
                Parts = new List<BomSummaryPartInput>()
            };

            var before_count = ctx.BomSummaryParts.Count();
            // test
            var bom_summaryService = new BomSummaryService(ctx);
            var payload = await bom_summaryService.CreateBomSummary(dto);

            // assert
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            var expectedError = "bom summary must have parts";
            Assert.Equal(expectedError, errorMessage);
        }
    }
}
