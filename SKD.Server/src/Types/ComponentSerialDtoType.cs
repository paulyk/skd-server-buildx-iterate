using HotChocolate.Types;
namespace SKD.Server;

public class ComponentSerialDtoType : ObjectType<ComponentSerialDTO> {
    protected override void Configure(IObjectTypeDescriptor<ComponentSerialDTO> descriptor) {
        descriptor.Field(t => t.VIN).Name("vin");
    }
}



