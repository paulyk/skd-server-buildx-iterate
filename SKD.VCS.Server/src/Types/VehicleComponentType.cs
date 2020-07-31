using HotChocolate.Types;
using SKD.VCS.Model;

namespace SKD.VCS.Server {
    public class VehicleComponentType : ObjectType<VehicleComponent> {
        protected override void Configure(IObjectTypeDescriptor<VehicleComponent> descriptor) {
            descriptor.Field(t => t.ComponentScans).UseFiltering();
        }
    }
}