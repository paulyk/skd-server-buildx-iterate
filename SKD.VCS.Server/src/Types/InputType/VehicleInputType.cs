using HotChocolate.Types;
using SKD.VCS.Model;

namespace SKD.VCS.Server {

    public class VehicleInputType : InputObjectType<VehicleKitDTO> {
        protected override void Configure(IInputObjectTypeDescriptor<VehicleKitDTO> descriptor) {
            base.Configure(descriptor);
        }
    }
}

