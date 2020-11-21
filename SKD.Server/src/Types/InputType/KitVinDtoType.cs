using HotChocolate.Types;
using SKD.Model;

namespace SKD.Server {

    public class KitVinDtoType : InputObjectType<KitVinDTO> {
        protected override void Configure(IInputObjectTypeDescriptor<KitVinDTO> descriptor) {
            base.Configure(descriptor);
            descriptor.Field(t => t.VIN).Name("vin");
        }
    }
}

