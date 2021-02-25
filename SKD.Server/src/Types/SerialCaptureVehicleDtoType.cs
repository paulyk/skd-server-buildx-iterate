using HotChocolate.Types;
using SKD.Model;

namespace SKD.Server {

    public class SerialCaptureVehicleDTOType : ObjectType<BasicKitInfo> {
        protected override void Configure(IObjectTypeDescriptor<BasicKitInfo> descriptor) {
            descriptor.Field(t => t.VIN).Name("vin");
        }

    }
}

