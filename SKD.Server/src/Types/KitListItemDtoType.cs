namespace SKD.Server;

public class KitListItemDtoType : ObjectType<KitListItemDTO> {
    protected override void Configure(IObjectTypeDescriptor<KitListItemDTO> descriptor) {
        descriptor.Field(t => t.VIN).Name("vin");
    }
}

