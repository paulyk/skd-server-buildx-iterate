using HotChocolate.Types;
using SKD.Model;

namespace SKD.Server {
  public class KitVinType: ObjectType<KitVin> {
    protected override void Configure(IObjectTypeDescriptor<KitVin> descriptor) {
      descriptor.Field(t => t.VIN).Name("vin");
    }
  }
}