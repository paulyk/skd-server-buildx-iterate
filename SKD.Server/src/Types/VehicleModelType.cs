using HotChocolate.Types;
using SKD.Model;

namespace SKD.Server {
  public class VehicleModelType: ObjectType<VehicleModel> {
    protected override void Configure(IObjectTypeDescriptor<VehicleModel> descriptor) {
      descriptor.Field(t => t.ModelComponents);
    }
  }
}