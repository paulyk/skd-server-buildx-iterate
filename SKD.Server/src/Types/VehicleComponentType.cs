using HotChocolate.Types;
using SKD.Model;

namespace SKD.Server {
    public class VehicleComponentType : ObjectType<KitComponent> {
        protected override void Configure(IObjectTypeDescriptor<KitComponent> descriptor) {
            descriptor.Field(t => t.ComponentSerials).UseFiltering();
        }
    }
}