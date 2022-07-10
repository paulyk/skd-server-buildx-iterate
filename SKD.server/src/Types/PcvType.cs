namespace SKD.Server;
public class PcvType : ObjectType<PCV> {
    protected override void Configure(IObjectTypeDescriptor<PCV> descriptor) {
        descriptor.Field(t => t.PcvComponents);
    }
}
