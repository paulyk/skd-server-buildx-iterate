#nullable enable

namespace SKD.Service;

public class VerifySerialService {
    private readonly SkdContext context;
    private readonly DcwsService dcwsService;
    private readonly DCWSResponseService dcwsResponseService;

    public VerifySerialService(SkdContext ctx, DcwsService dcwsService, DCWSResponseService dcwsResponseService) {
        this.context = ctx;
        this.dcwsService = dcwsService;
        this.dcwsResponseService = dcwsResponseService;
    }

    public async Task<MutationResult<DcwsResponse>> VerifyComponentSerial(
        Guid kitComponentId
    ) {
        var result = new MutationResult<DcwsResponse>(null);
        result.Errors = await ValidateVerifyComponentSerial(kitComponentId);
        if (result.Errors.Any()) {
            return result;
        }

        var kc = await context.KitComponents
            .Include(t => t.Component)
            .Include(t => t.ComponentSerials)
            .Where(t => t.Id == kitComponentId).FirstAsync();

        var componentSerial = kc.ComponentSerials
            .OrderByDescending(t => t.CreatedAt)
            .Where(t => t.RemovedAt == null)
            .First();

        var input = new SubmitDcwsComponentInput {
            VIN = kc.Kit.VIN,
            ComponentTypeCode = kc.Component.Code,
            Serial1 = componentSerial.Serial1,
            Serial2 = componentSerial.Serial2
        };

        var submitDcwsComponentResponse = await dcwsService.SubmitDcwsComponent(input);
        var dcwsResponsePayload = await dcwsResponseService.SaveDcwsComponentResponse(new DcwsComponentResponseInput {
            VehicleComponentId = kitComponentId,
            ResponseCode = submitDcwsComponentResponse.ProcessExceptionCode,
        });

        return dcwsResponsePayload;
    }

    public async Task<List<Error>> ValidateVerifyComponentSerial(Guid kitComponentId) {
        List<Error> errors = new();

        var kc = await context.KitComponents
            .Include(t => t.Kit)
            .Include(t => t.Component)
            .Include(t => t.ComponentSerials).ThenInclude(t => t.DcwsResponses)
            .Where(t => t.Id == kitComponentId).FirstOrDefaultAsync();

        if (kc == null) {
            errors.Add(new Error("", $"Kit component not found for {kitComponentId}"));
            return errors;
        }

        if (kc.RemovedAt != null) {
            errors.Add(new Error("", $"Kit component marked removed for {kitComponentId}"));
            return errors;
        }

        if (String.IsNullOrWhiteSpace(kc.Kit.VIN)) {
            errors.Add(new Error("", $"kit does not have VIN {kc.Kit.KitNo}"));
            return errors;
        }

        if (!kc.Component.DcwsRequired) {
            errors.Add(new Error("", $"Component {kc.Component.Code} not required by DCWS"));
            return errors;
        }

        var latestComponentSerial = kc.ComponentSerials
            .OrderByDescending(t => t.CreatedAt)
            .Where(t => t.RemovedAt == null)
            .FirstOrDefault();

        if (latestComponentSerial == null) {
            errors.Add(new Error("", $"No component serial found for this kit component {kc.Kit.KitNo} {kc.Component.Code}"));
            return errors;
        }

        var dcwsResponse = latestComponentSerial.DcwsResponses
            .OrderByDescending(t => t.CreatedAt)
            .Where(t => t.RemovedAt == null)
            .FirstOrDefault();

        if (dcwsResponse != null && DCWSResponseService.IsSuccessProcessExceptionCode(dcwsResponse.ProcessExcptionCode)) {
            errors.Add(new Error("", $"Component already verfied j {kc.Kit.KitNo} {kc.Component.Code}"));
            return errors;
        }

        return errors;
    }
}

