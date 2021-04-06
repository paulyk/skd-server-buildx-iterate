using HotChocolate.Types;
using SKD.Model;

namespace SKD.Server {

    public class AssignKitVinInputType : InputObjectType<ImportVinInput.KitVin> {
        protected override void Configure(IInputObjectTypeDescriptor<ImportVinInput.KitVin> descriptor) {
            base.Configure(descriptor);
            descriptor.Field(t => t.VIN).Name("vin");
        }
    }
}

