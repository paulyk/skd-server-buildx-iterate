using System.Threading.Tasks;
using ServiceReference;
using static ServiceReference.HTTPDataCollectionSoapClient;

namespace SKD.Dcws {
    public class DcwsService {
        private HTTPDataCollectionSoapClient client;

        public DcwsService(string serviceAddress) {
            this.client = new HTTPDataCollectionSoapClient(EndpointConfiguration.HTTPDataCollectionSoap, serviceAddress);
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

            var serial1 = input.Serial1;
            
            // Serial1: Reformat if TR otherwise use as is.
            if (input.ComponentTypeCode == "TR") {
                var formatter = new TR_SerialFormatter();
                var result = formatter.Format_TR_Serial(input.Serial1);
                if (!result.Success) {
                    throw new System.Exception($"Error transforming TR serial");
                }
                serial1 = result.Serial;
            }

            var payload = await client.SaveCDCComponentAsync(
                vin: input.VIN,
                componentTypeCode: input.ComponentTypeCode,
                scan1: serial1,
                scan2: input.Serial2,
                //
                acceptIfComponentNotRequired: false,
                acceptIfInvalidScan: false,
                acceptIfKnownBadComponent: false,
                acceptIfNotVerified: false,
                acceptIfPartAlreadyInstalled: false,
                acceptIfVINNotFound: false,
                acceptIfWrongComponent: false
            );

            var processExecption = payload.Body.SaveCDCComponentResult.ProcessException;
            return new SubmitDcwsComponentRespnse {
                VIN = input.VIN,
                ComponentTypeCode = input.ComponentTypeCode,
                Serial1 = input.Serial1,
                Serial2 = input.Serial2,
                ProcessExceptionCode = processExecption
            };
        }
    }
}