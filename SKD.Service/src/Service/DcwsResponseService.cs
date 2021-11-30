#nullable enable

namespace SKD.Service;

public class DCWSResponseService {

    private readonly ICollection<string> SuccessProcessExceptionCodes = new List<string> {
            "NONE", "REPAIR", "KNOWNBAD", "CHARACTERIZATIONMISSING", "CHARACTERIZATIONERROR"
        };
    private readonly SkdContext context;

    public DCWSResponseService(SkdContext ctx) {
        this.context = ctx;
    }

    public async Task<MutationPayload<DcwsResponse>> SaveDcwsComponentResponse(DcwsComponentResponseInput input) {
        MutationPayload<DcwsResponse> payload = new();
        payload.Errors = await ValidateDcwsComponentResponse<DcwsComponentResponseInput>(input);
        if (payload.Errors.Any()) {
            return payload;
        }

        var kitComponent = await context.KitComponents
            .Include(t => t.ComponentSerials).ThenInclude(t => t.DcwsResponses)
            .Where(t => t.Id == input.VehicleComponentId)
            .FirstAsync();

        var componentSerial = kitComponent.ComponentSerials.Where(t => t.RemovedAt == null).First();

        // skip if duplicate
        var duplicate = componentSerial.DcwsResponses.ToList()
            .OrderByDescending(t => t.CreatedAt)
            .Where(t => t.RemovedAt == null)
            .Where(t => t.ProcessExcptionCode == input.ResponseCode)
            .FirstOrDefault();
        if (duplicate != null) {
            payload.Payload = duplicate;
            return payload;
        }


        // mark existing reponses as removed
        componentSerial.DcwsResponses.Where(t => t.RemovedAt == null).ToList().ForEach(t => {
            t.RemovedAt = DateTime.UtcNow;
        });

        var response = new DcwsResponse {
            ProcessExcptionCode = input.ResponseCode,
            ErrorMessage = input.ErrorMessage,
            ComponentSerialId = componentSerial.Id,
            DcwsSuccessfulSave = IsSuccessProcessExceptionCode(input.ResponseCode)
        };

        // update denormalized values
        componentSerial.VerifiedAt = response.DcwsSuccessfulSave ? DateTime.UtcNow : (DateTime?)null;
        kitComponent.VerifiedAt = response.DcwsSuccessfulSave ? DateTime.UtcNow : (DateTime?)null;

        context.DCWSResponses.Add(response);

        await context.SaveChangesAsync();
        payload.Payload = response;
        return payload;
    }

    public async Task<List<Error>> ValidateDcwsComponentResponse<T>(DcwsComponentResponseInput input) where T : DcwsComponentResponseInput {
        var errors = new List<Error>();

        var kitComponent = await context.KitComponents
            .Include(t => t.ComponentSerials)
            .Where(t => t.Id == input.VehicleComponentId)
            .FirstOrDefaultAsync();

        if (kitComponent == null) {
            errors.Add(new Error("", $"kit component not found"));
            return errors;
        }

        if (kitComponent.RemovedAt != null) {
            errors.Add(new Error("", $"kit component removed for ComponentScanId = {input.VehicleComponentId}"));
            return errors;
        }

        var componentSerial = kitComponent.ComponentSerials
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

        return errors;
    }

    bool IsSuccessProcessExceptionCode(string processExceptionCode) {
        return SuccessProcessExceptionCodes.Any(code => code.ToLower() == processExceptionCode.ToLower());
    }
}