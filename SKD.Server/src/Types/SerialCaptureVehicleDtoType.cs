using HotChocolate.Types;
using SKD.Model;

namespace SKD.Server {

    public class SerialCaptureVehicleDTOType : ObjectType<SerialCaptureVehicleDTO> {
        protected override void Configure(IObjectTypeDescriptor<SerialCaptureVehicleDTO> descriptor) {
            descriptor.Field(t => t.VIN).Name("vin");
        }

    }
}

