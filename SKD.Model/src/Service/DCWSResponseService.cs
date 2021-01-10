#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.Model {

    public class DCWSResponseService {

        private readonly ICollection<string> ComponentSavedReponseCodes = new List<string> {
            "NONE", "REPAIR", "KNOWNBAD", "CHARACTERIZATIONMISSING", "CHARACTERIZATIONERROR"
        };
        private readonly SkdContext context;

        public DCWSResponseService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<DCWSResponse>> CreateDCWSResponse(DCWWResponseInput input) {
            var response = new DCWSResponse {
                ResponseCode = input.ResponseCode,
                ErrorMessage = input.ErrorMessage,
                ComponentScanId = input.ComponentScanId,
                ComponentSerial = await context.ComponentSerials
                    .Include(t => t.VehicleComponent)
                    .FirstOrDefaultAsync(t => t.Id == input.ComponentScanId),
                DcwsSuccessfulSave = IsDcwsSuccessfulSaveResonseCode(input.ResponseCode)
            };
            var payload = new MutationPayload<DCWSResponse>(response);
            payload.Errors = await ValidateDCWSResponse<DCWWResponseInput>(input);

            if (payload.Errors.Any()) {
                return payload;
            }

            // update denormalized values
            response.ComponentSerial.VerifiedAt = response.DcwsSuccessfulSave ? DateTime.UtcNow : (DateTime?)null;
            response.ComponentSerial.VehicleComponent.VerifiedAt = response.DcwsSuccessfulSave ? DateTime.UtcNow : (DateTime?)null;

            context.DCWSResponses.Add(payload.Entity);
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<List<Error>> ValidateDCWSResponse<T>(DCWWResponseInput dto) where T : DCWWResponseInput {
            var errors = new List<Error>();

            var componentScan = await context.ComponentSerials
                .Include(t => t.DCWSResponses)
                .Include(t => t.VehicleComponent).ThenInclude(t => t.Vehicle)
                .FirstOrDefaultAsync(t => t.Id == dto.ComponentScanId);

            if (componentScan == null) {
                errors.Add(new Error("", $"component scan not found for ComponentScanId = {dto.ComponentScanId}"));
                return errors;
            }

            if (componentScan.VehicleComponent.RemovedAt != null) {
                errors.Add(new Error("", $"vehicle component removed for ComponentScanId = {dto.ComponentScanId}"));
                return errors;
            }

            if (String.IsNullOrEmpty(dto.ResponseCode)) {
                errors.Add(new Error("ResponseCode", "response code required"));
                return errors;
            }

            // skip if duplicate
            var duplicate  = await context.DCWSResponses
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync(
                    t => t.ComponentScanId == dto.ComponentScanId && 
                    t.ResponseCode == dto.ResponseCode &&
                    t.ErrorMessage == dto.ErrorMessage);

            if (duplicate != null) {
                errors.Add(new Error("", "duplicate"));
            }

            return errors;
        }

        bool IsDcwsSuccessfulSaveResonseCode(string responseCode) {
            return ComponentSavedReponseCodes.Any(code => code.ToLower() == responseCode.ToLower());
        }
    }
}
