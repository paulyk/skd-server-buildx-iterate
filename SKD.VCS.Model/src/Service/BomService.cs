#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.VCS.Model {

    public class BomService {
        private readonly SkdContext context;

        public BomService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<Bom>> CreateBom(BomDTO dto) {
            var bom = new Bom() {
                SequenceNo = dto.SequenceNo,
                ProductionPlant = await context.ProductionPlants.FirstOrDefaultAsync(t => t.Code == dto.ProductionPlantCode),
                Lots = dto.Lots.Select(lotDTO => new BomLot {
                    LotNo = lotDTO.LotNo,
                    Parts = lotDTO.Parts.Select(partDTO => new BomPart {
                        PartNo = partDTO.PartNo,
                        PartDesc = partDTO.PartDesc,
                        KitNo = partDTO.KitNo,
                        Quantity = partDTO.Quantity
                    }).ToList()
                }).ToList()
            };

            var payload = new MutationPayload<Bom>(bom);

            payload.Errors = await ValidateBomDTO<BomDTO>(dto);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            context.Boms.Add(bom);
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<List<Error>> ValidateBomDTO<T>(BomDTO dto) where T : BomDTO {
            var errors = new List<Error>();

            var duplicate = await context.Boms.AnyAsync(t => t.SequenceNo == dto.SequenceNo);
            if (duplicate) {
                errors.Add(new Error("", "duplicate bom sequence number"));
                return errors;
            }

            // production plant
            var productionPlantExists = await context.ProductionPlants.AnyAsync(t => t.Code == dto.ProductionPlantCode);
            if (!productionPlantExists) {
                errors.Add(new Error("productionPlantCode", $"production plant not found for code: {dto.ProductionPlantCode}"));
            }

            // shipment dto must have lot + invoice + parts
            if (!dto.Lots.Any()) {
                errors.Add(new Error("", "bom must have lots"));
                return errors;
            }

            if (dto.Lots.Any(t => t.Parts.Count() == 0)) {
                errors.Add(new Error("", "bom lots must have parts"));
                return errors;
            }

            if (dto.Lots.Any(t => t.Parts.Any(t => string.IsNullOrEmpty(t.PartNo)))) {
                errors.Add(new Error("", "bom parts must have part number"));
                return errors;
            }

            if (dto.Lots.Any(t => t.Parts.Any(t => string.IsNullOrEmpty(t.PartDesc)))) {
                errors.Add(new Error("", "bom parts must have part description"));
                return errors;
            }

            if (dto.Lots.Any(t => t.Parts.Any(t => t.Quantity > 0))) {
                errors.Add(new Error("", "bom parts must have non zero quantity"));
                return errors;
            }   

            return errors;
        }
    }
}
