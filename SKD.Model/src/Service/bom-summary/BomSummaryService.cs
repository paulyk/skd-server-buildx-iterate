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

        public async Task<MutationPayload<BomSummary>> CreateBomSummary(BomSummaryInput input) {
            var payload = new MutationPayload<BomSummary>(null);

            payload.Errors = await ValidateBomDTO<BomSummaryInput>(input);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            var bomSummary = new BomSummary() {
                Sequence = input.Sequence,
                Parts = input.Parts.Select(partDTO => new BomSummaryPart {
                    LotNo = partDTO.LotNo,
                    PartNo = partDTO.PartNo,
                    PartDesc = partDTO.PartDesc,
                    Quantity = partDTO.Quantity
                }).ToList()
            };
            payload.Entity = bomSummary;

            // ensure plant code
            var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode);
            if (plant == null) {
                plant = new Plant { Code = input.PlantCode, Name = input.PlantCode };
                context.Plants.Add(plant);
            }
            plant.BomSummaries.Add(bomSummary);

            //
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<List<Error>> ValidateBomDTO<T>(BomSummaryInput input) where T : BomSummaryInput {
            var errors = new List<Error>();

            if (String.IsNullOrEmpty(input.PlantCode) || input.PlantCode.Length != EntityFieldLen.Plant_Code) {
                errors.Add(new Error("", "plant code required"));
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
