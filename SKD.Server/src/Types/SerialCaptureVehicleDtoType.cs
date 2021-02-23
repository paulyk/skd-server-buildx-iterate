using HotChocolate.Types;
using SKD.Model;

namespace SKD.Server {

    public class SerialCaptureVehicleDTOType : ObjectType<SerialCaptureKitDTO> {
        protected override void Configure(IObjectTypeDescriptor<SerialCaptureKitDTO> descriptor) {
            descriptor.Field(t => t.VIN).Name("vin");
        }

    }
}

