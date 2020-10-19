#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.VCS.Model {

    public class DCWSResponseService {

        private readonly ICollection<string> ComponentSavedReponseCodes = new List<string> {
            "NONE", "REPAIR", "KNOWNBAD", "CHARACTERIZATIONMISSING", "CHARACTERIZATIONERROR"
        };
        private readonly SkdContext context;

        public DCWSResponseService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<DCWSResponse>> CreateDCWSResponse(DCWWResponseDTO dto) {
            var response = new DCWSResponse {
                ResponseCode = dto.ResponseCode,
                ErrorMessage = dto.ErrorMessage,
                ComponentScanId = dto.ComponentScanId,
                ComponentScan = await context.ComponentScans
                    .Include(t => t.VehicleComponent)
                    .FirstOrDefaultAsync(t => t.Id == dto.ComponentScanId),
                DcwsSuccessfulSave = IsDcwsSuccessfulSaveResonseCode(dto.ResponseCode)
            };
            var payload = new MutationPayload<DCWSResponse>(response);
            payload.Errors = await ValidateDCWSResponse<DCWWResponseDTO>(dto);

            if (payload.Errors.Any()) {
                return payload;
            }

            // update denormalized values
            response.ComponentScan.AcceptedAt = response.DcwsSuccessfulSave ? DateTime.UtcNow : (DateTime?)null;
            response.ComponentScan.VehicleComponent.ScanVerifiedAt = response.DcwsSuccessfulSave ? DateTime.UtcNow : (DateTime?)null;

            context.DCWSResponses.Add(payload.Entity);
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<List<Error>> ValidateDCWSResponse<T>(DCWWResponseDTO dto) where T : DCWWResponseDTO {
            var errors = new List<Error>();

            var componentScan = await context.ComponentScans
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
            }

            return errors;
        }

        bool IsDcwsSuccessfulSaveResonseCode(string responseCode) {
            return ComponentSavedReponseCodes.Any(code => code.ToLower() == responseCode.ToLower());
        }
    }
}
