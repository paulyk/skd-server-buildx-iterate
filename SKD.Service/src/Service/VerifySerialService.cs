using System;
using System.Threading.Tasks;
using SKD.Model;
using SKD.Service;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SKD.Dcws;

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

    public async Task<MutationPayload<DcwsResponse>> VerifyComponentSerial(
        Guid kitComponentId
    ) {
        var payload = new MutationPayload<DcwsResponse>(null);

        var kc = await context.KitComponents.Where(t => t.Id == kitComponentId).FirstOrDefaultAsync();
        if (kc == null) {
            payload.Errors.Add(new Error("", $"Kit component not found for {kitComponentId}"));
            return payload;
        }
        if (kc.RemovedAt != null) {
            payload.Errors.Add(new Error("", $"Kit component marked removed for {kitComponentId}"));
            return payload;
        }

        var componentSerial = await context.ComponentSerials
            .Include(t => t.KitComponent).ThenInclude(t => t.Kit)
            .Include(t => t.KitComponent).ThenInclude(t => t.Component)
            .OrderByDescending(t => t.CreatedAt)
            .Where(t => t.KitComponentId == kitComponentId)
            .Where(t => t.RemovedAt == null)
            .FirstOrDefaultAsync();

        if (componentSerial == null) {
            payload.Errors.Add(new Error("", $"No component serial found for this kit component {kitComponentId}"));
            return payload;
        }

        var input = new SubmitDcwsComponentInput {
            VIN = componentSerial.KitComponent.Kit.VIN,
            ComponentTypeCode = componentSerial.KitComponent.Component.Code,
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
}

