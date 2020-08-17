using HotChocolate.Types;
using SKD.VCS.Model;

namespace SKD.VCS.Server {

    public class VehicleType : ObjectType<Vehicle> {
        protected override void Configure(IObjectTypeDescriptor<Vehicle> descriptor) {
          descriptor.Field(t => t.VIN).Name("vin");          
        }
    }
}

