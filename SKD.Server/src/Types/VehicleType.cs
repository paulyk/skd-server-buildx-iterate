using HotChocolate.Types;
using SKD.Model;

namespace SKD.Server {

    public class VehicleType : ObjectType<Kit> {
        protected override void Configure(IObjectTypeDescriptor<Kit> descriptor) {
          descriptor.Field(t => t.VIN).Name("vin");          
        }
    }
}

