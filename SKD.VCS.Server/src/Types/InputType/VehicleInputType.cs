using HotChocolate.Types;
using SKD.VCS.Model;

namespace SKD.VCS.Server {

    public class VehicleInputType : InputObjectType<VehicleDTO> {
        protected override void Configure(IInputObjectTypeDescriptor<VehicleDTO> descriptor) {
            base.Configure(descriptor);
            descriptor.Field(t => t.VIN).Name("vin");
        }
    }
}

