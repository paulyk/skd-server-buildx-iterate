using HotChocolate.Types;
using SKD.VCS.Model;

namespace SKD.VCS.Server {

    public class KitVinDtoType : InputObjectType<KitVinDTO> {
        protected override void Configure(IInputObjectTypeDescriptor<KitVinDTO> descriptor) {
            base.Configure(descriptor);
            descriptor.Field(t => t.VIN).Name("vin");
        }
    }
}

