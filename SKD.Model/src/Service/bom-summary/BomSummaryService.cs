#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.Model {

    public class BomSummaryService {
        private readonly SkdContext context;

        public BomSummaryService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<BomSummaryOverviewDTO>> CreateBomSummary(BomSummaryInput input) {
            var payload = new MutationPayload<BomSummaryOverviewDTO>(null);
            payload.Errors = await ValidateBomDTO<BomSummaryInput>(input);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            var bomSummary = new BomSummary() {
                Plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode),
                Sequence = input.Sequence,
                Parts = input.Parts.Select(partDTO => new BomSummaryPart {
                    LotNo = partDTO.LotNo,
                    PartNo = partDTO.PartNo,
                    PartDesc = partDTO.PartDesc,
                    Quantity = partDTO.Quantity
                }).ToList()
            };

            // ensure plant code
            context.BomSummaries.Add(bomSummary);
            await context.SaveChangesAsync();
            payload.Entity = new BomSummaryOverviewDTO {
                Id = bomSummary.Id,
                Sequence = bomSummary.Sequence,
                PlantCode = bomSummary.Plant.Code,
                LotCount = bomSummary.Parts.Select(x => x.LotNo).Distinct().Count(),
                LotPartCount = bomSummary.Parts.Count(),
                CreatedAt = bomSummary.CreatedAt
            };
            return payload;
        }

        public async Task<List<Error>> ValidateBomDTO<T>(BomSummaryInput input) where T : BomSummaryInput {
            var errors = new List<Error>();

            var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode);
            if (plant == null) {
                errors.Add(new Error("PlantCode", $"plant not found  {input.PlantCode}"));
                return errors;
            }

            var duplicate = await context.BomSummaries.AnyAsync(t => t.Sequence == input.Sequence);
            if (duplicate) {
                errors.Add(new Error("", "duplicate bom summary sequence number"));
                return errors;
            }

            if (!input.Parts.Any()) {
                errors.Add(new Error("", "bom summary must have parts"));
                return errors;
            }

            // duplicate lotNo + Part
            var duplicateLotParts = input.Parts.GroupBy(t => new { t.LotNo, t.PartNo })
                .Any(g => g.Count() > 1);
            if (duplicateLotParts) {
                errors.Add(new Error("", "bom summary cannot have duplicate Lot + Part numbers"));
                return errors;
            }

            if (input.Parts.Any(t => string.IsNullOrEmpty(t.LotNo))) {
                errors.Add(new Error("", "bom summary parts must have lot number"));
                return errors;
            }

            if (input.Parts.Any(t => string.IsNullOrEmpty(t.PartNo))) {
                errors.Add(new Error("", "bom summary parts must have part number"));
                return errors;
            }

            if (input.Parts.Any(t => string.IsNullOrEmpty(t.PartDesc))) {
                errors.Add(new Error("", "bom summary parts must have part number"));
                return errors;
            }

            if (input.Parts.Any(t => t.Quantity <= 0)) {
                errors.Add(new Error("", "bom summary parts quantity must be greater than 0"));
                return errors;
            }

            return errors;
        }
    

    }
}
