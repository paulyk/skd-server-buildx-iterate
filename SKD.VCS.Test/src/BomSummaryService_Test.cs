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
        private async Task cannot_create_bom_summary_with_duplicate_lot_and_part() {
            // setup
            var lotNo = Util.RandomString(EntityFieldLen.BomPart_LotNo);

            var dto = new BomSummaryDTO() {
                SequenceNo = "0001",
                Parts = new List<BomSummaryPartDTO> {
                    new BomSummaryPartDTO {
                        LotNo = lotNo,
                        PartNo = "0001",
                        PartDesc = "part 1",
                        Quantity = 1
                    },
                    new BomSummaryPartDTO {
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

        private async Task can_create_bom_summary() {
            // setup
            var dto = new BomSummaryDTO() {
                SequenceNo = "0001",
                Parts = new List<BomSummaryPartDTO> {
                    new BomSummaryPartDTO {
                        LotNo = Util.RandomString(EntityFieldLen.BomPart_LotNo),
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
        private async Task cannot_create_bom_summary_with_no_pards() {
            // setup
            var dto = new BomSummaryDTO() {
                SequenceNo = "0001",
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