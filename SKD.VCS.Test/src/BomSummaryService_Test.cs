using System;
using System.Collections.Generic;
using SKD.VCS.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.VCS.Test {
    public class BomSummaryService_Test : TestBase {

        private SkdContext ctx;
        public BomSummaryService_Test() {
            ctx = GetAppDbContext();
        }

        [Fact]
        private async Task can_create_bom_summary() {
            // setup
            var plantCode = "HD001";
            Gen_ProductionPlant(ctx, plantCode);

            var dto = new BomSummaryDTO() {
                SequenceNo = "0001",
                ProductionPlantCode = plantCode,
                Parts = new List<BomSummaryPartDTO> {
                    new BomSummaryPartDTO {
                        PartNo = "0001",
                        PartDesc = "part 1",
                        Quantity = 1
                    }
                }
            };

            var before_count = ctx.BomSummaryParts.Count();
            // test
            var bomService = new BomSummaryService(ctx);
            var payload = await bomService.CreateBomSummary(dto);

            // assert
            var after_count = ctx.BomSummaryParts.Count();
            Assert.Equal(1, after_count);
        }

        [Fact]
        private async Task cannot_create_bom_summary_if_production_plant_code_not_found() {
            // setup
            var plantCode = Util.RandomString(EntityFieldLen.ProductionPlant_Code);
            Gen_ProductionPlant(ctx, plantCode);

            var dto = new BomSummaryDTO() {
                SequenceNo = "0001",
                ProductionPlantCode = Util.RandomString(EntityFieldLen.ProductionPlant_Code),
                Parts = new List<BomSummaryPartDTO> {
                    new BomSummaryPartDTO {
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
            var expectedError = "production plant not found for code";
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            errorMessage = (errorMessage ?? "").Substring(0, expectedError.Length);
            Assert.Equal(expectedError, errorMessage);
        }

        [Fact]
        private async Task cannot_create_bom_summary_with_no_pards() {
            // setup
            var plantCode = "HD001";
            Gen_ProductionPlant(ctx, plantCode);

            var dto = new BomSummaryDTO() {
                SequenceNo = "0001",
                ProductionPlantCode = plantCode,
                Parts = new List<BomSummaryPartDTO>()
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
