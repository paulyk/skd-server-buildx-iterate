#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.Model {

    public class DCWSResponseService {

        private readonly ICollection<string> SuccessProcessExceptionCodes = new List<string> {
            "NONE", "REPAIR", "KNOWNBAD", "CHARACTERIZATIONMISSING", "CHARACTERIZATIONERROR"
        };
        private readonly SkdContext context;

        public DCWSResponseService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<DcwsResponse>> SaveDcwsComponentResponse(DcwsComponentResponseInput input) {
            var payload = new MutationPayload<DcwsResponse>(null);
            payload.Errors = await ValidateDcwsComponentResponse<DcwsComponentResponseInput>(input);
            if (payload.Errors.Any()) {
                return payload;
            }

            var vehicleComponent = await context.VehicleComponents
                .Include(t => t.ComponentSerials)
                .Where(t => t.Id == input.VehicleComponentId)
                .FirstOrDefaultAsync();
            var componentSerial = vehicleComponent.ComponentSerials.Where(t =>t.RemovedAt == null).First();

            var response = new DcwsResponse {
                ProcessExcptionCode = input.ResponseCode,
                ErrorMessage = input.ErrorMessage,
                ComponentSerialId = componentSerial.Id,
                DcwsSuccessfulSave = IsSuccessProcessExceptionCode(input.ResponseCode)
            };
            payload.Entity = response;

            // update denormalized values
            componentSerial.VerifiedAt = response.DcwsSuccessfulSave ? DateTime.UtcNow : (DateTime?)null;
            vehicleComponent.VerifiedAt = response.DcwsSuccessfulSave ? DateTime.UtcNow : (DateTime?)null;

            context.DCWSResponses.Add(payload.Entity);
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<List<Error>> ValidateDcwsComponentResponse<T>(DcwsComponentResponseInput input) where T : DcwsComponentResponseInput {
            var errors = new List<Error>();

            var vehicleComponent = await context.VehicleComponents
                .Include(t => t.ComponentSerials)
                .Where(t => t.Id == input.VehicleComponentId)
                .FirstOrDefaultAsync();

            if (vehicleComponent.RemovedAt != null) {
                errors.Add(new Error("", $"vehicle component removed for ComponentScanId = {input.VehicleComponentId}"));
                return errors;
            }

            var componentSerial = vehicleComponent.ComponentSerials
                .Where(t => t.RemovedAt == null)
                .First();

            if (componentSerial == null) {
                errors.Add(new Error("", $"component scan not found for ComponentScanId = {input.VehicleComponentId}"));
                return errors;
            }

            if (input.ResponseCode is null or "") {
                errors.Add(new Error("ResponseCode", "response code required"));
                return errors;
            }

            // skip if duplicate
            var duplicate  = await context.DCWSResponses
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync(
                    t => t.ComponentSerialId == componentSerial.Id && 
                    t.ProcessExcptionCode == input.ResponseCode &&
                    t.ErrorMessage == input.ErrorMessage);

            if (duplicate != null) {
                errors.Add(new Error("", "duplicate"));
            }

            return errors;
        }

        bool IsSuccessProcessExceptionCode(string processExceptionCode) {
            return SuccessProcessExceptionCodes.Any(code => code.ToLower() == processExceptionCode.ToLower());
        }
    }
}
