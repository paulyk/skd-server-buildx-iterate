using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace SKD.Model {

    public class PartnerStatusService {

        private readonly SkdContext context;

        public PartnerStatusService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<QueryPayload<PartnerStatusInput>> GetPartnerStaus(PartnerStatusInput input) {
            var dto = new PartnerStatusInput {
                RunDate = input.RunDate,
                PlantCode = input.PlantCode
            };
            var payload = new QueryPayload<PartnerStatusInput>(dto);
            payload.Errors = await ValidateGetPartnerStatus(input);
            if (payload.Errors.Any()) {
                return payload;
            }

            return payload;
        }

        public async Task<List<Error>> ValidateGetPartnerStatus(PartnerStatusInput dto) {
            var errors = new List<Error>();

            var plantExists = await context.Plants.AnyAsync(t => t.Code == dto.PlantCode);
            if (!plantExists) {
                errors.Add(new Error("plantCode", "plant code not found"));
            }

            return errors;
        }
    }
}