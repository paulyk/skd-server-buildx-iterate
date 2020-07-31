using HotChocolate.Types;
using SKD.VCS.Model;

namespace SKD.VCS.Server {
  public class VehicleModelType: ObjectType<VehicleModel> {
    protected override void Configure(IObjectTypeDescriptor<VehicleModel> descriptor) {
      descriptor.Field(t => t.ModelComponents).UseSelection().UseFiltering();
    }
  }
}