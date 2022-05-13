using ServiceReference;
using static ServiceReference.HTTPDataCollectionSoapClient;

namespace SKD.Dcws;
public class DcwsService {
    private readonly HTTPDataCollectionSoapClient client;

    public DcwsService(string serviceAddress) {
        var config = EndpointConfiguration.HTTPDataCollectionSoap;
        this.client = new HTTPDataCollectionSoapClient(config, serviceAddress);
    }

    public async Task<string> GetServiceVersion() {
        var result = await client.GetVersionAsync();
        return result.Body.GetVersionResult.DCWSCOMVersion;
    }
    public async Task<bool> CanConnectToService() {
        await client.CheckConnectivityAsync();
        return true;
    }

    public async Task<SubmitDcwsComponentRespnse> SubmitDcwsComponent(SubmitDcwsComponentInput input) {


        var result = await client.SaveCDCComponentAsync(
            vin: input.VIN,
            componentTypeCode: input.ComponentTypeCode,
            scan1: input.Serial1,
            scan2: input.Serial2,
            //
            acceptIfComponentNotRequired: false,
            acceptIfInvalidScan: false,
            acceptIfKnownBadComponent: false,
            acceptIfNotVerified: false,
            acceptIfPartAlreadyInstalled: true,
            acceptIfVINNotFound: false,
            acceptIfWrongComponent: false
        );

        var processExecption = result.Body.SaveCDCComponentResult.ProcessException;
        return new SubmitDcwsComponentRespnse {
            VIN = input.VIN,
            ComponentTypeCode = input.ComponentTypeCode,
            Serial1 = input.Serial1,
            Serial2 = input.Serial2,
            ProcessExceptionCode = processExecption
        };
    }
}
