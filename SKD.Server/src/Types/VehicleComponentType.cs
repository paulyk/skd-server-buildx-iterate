using HotChocolate.Types;
using SKD.Model;

namespace SKD.Server {
    public class VehicleComponentType : ObjectType<VehicleComponent> {
        protected override void Configure(IObjectTypeDescriptor<VehicleComponent> descriptor) {
            descriptor.Field(t => t.ComponentSerials).UseFiltering();
        }
    }
}