using HotChocolate.Types;
using SKD.Model;

namespace SKD.Server {

    public class KitVinDtoType : InputObjectType<KitVinInput> {
        protected override void Configure(IInputObjectTypeDescriptor<KitVinInput> descriptor) {
            base.Configure(descriptor);
            descriptor.Field(t => t.VIN).Name("vin");
        }
    }
}

