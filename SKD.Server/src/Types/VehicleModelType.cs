using HotChocolate.Types;
using SKD.Model;

namespace SKD.Server;
public class VehicleModelType : ObjectType<PCV> {
    protected override void Configure(IObjectTypeDescriptor<PCV> descriptor) {
        descriptor.Field(t => t.PcvComponents);
    }
}
