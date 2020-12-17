using HotChocolate.Types;
using SKD.Model;

namespace SKD.Server {

    public class AssignKitVinInputType : InputObjectType<AssignKitVinInput.KitVin> {
        protected override void Configure(IInputObjectTypeDescriptor<AssignKitVinInput.KitVin> descriptor) {
            base.Configure(descriptor);
            descriptor.Field(t => t.VIN).Name("vin");
        }
    }
}

