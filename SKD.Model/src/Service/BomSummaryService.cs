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

        public async Task<MutationPayload<BomSummary>> CreateBomSummary(BomSummaryInput dto) {
            var bomSummary = new BomSummary() {
                SequenceNo = dto.SequenceNo,
                Parts = dto.Parts.Select(partDTO => new BomSummaryPart {
                    LotNo = partDTO.LotNo,
                    PartNo = partDTO.PartNo,
                    PartDesc = partDTO.PartDesc,
                    Quantity = partDTO.Quantity
                }).ToList()
            };

            var payload = new MutationPayload<BomSummary>(bomSummary);

            payload.Errors = await ValidateBomDTO<BomSummaryInput>(dto);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            context.BomSummaries.Add(bomSummary);
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<List<Error>> ValidateBomDTO<T>(BomSummaryInput dto) where T : BomSummaryInput {
            var errors = new List<Error>();

            var duplicate = await context.BomSummaries.AnyAsync(t => t.SequenceNo == dto.SequenceNo);
            if (duplicate) {
                errors.Add(new Error("", "duplicate bom summary sequence number"));
                return errors;
            }

            if (!dto.Parts.Any()) {
                errors.Add(new Error("", "bom summary must have parts"));
                return errors;
            }

            // duplicate lotNo + Part
            var duplicateLotParts = dto.Parts.GroupBy(t => new { t.LotNo, t.PartNo})
                .Any(g => g.Count() > 1);
            if (duplicateLotParts) {
                errors.Add(new Error("", "bom summary cannot have duplicate Lot + Part numbers"));
                return errors;
            }

            if (dto.Parts.Any(t => string.IsNullOrEmpty(t.LotNo))) {
                errors.Add(new Error("", "bom summary parts must have lot number"));
                return errors;
            }

            if (dto.Parts.Any(t => string.IsNullOrEmpty(t.PartNo))) {
                errors.Add(new Error("", "bom summary parts must have part number"));
                return errors;
            }

            if (dto.Parts.Any(t => string.IsNullOrEmpty(t.PartDesc))) {
                errors.Add(new Error("", "bom summary parts must have part number"));
                return errors;
            }

            if (dto.Parts.Any(t => t.Quantity <= 0)) {
                errors.Add(new Error("", "bom summary parts quantity must be greater than 0"));
                return errors;
            }

            return errors;
        }
    }
}
