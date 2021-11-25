namespace SKD.Server;

public class VinFileInputType : InputObjectType<VinFile.VinFileKit> {
    protected override void Configure(IInputObjectTypeDescriptor<VinFile.VinFileKit> descriptor) {
        base.Configure(descriptor);
        descriptor.Field(t => t.VIN).Name("vin");
    }
}

public class VinFileType : ObjectType<VinFile.VinFileKit> {
    protected override void Configure(IObjectTypeDescriptor<VinFile.VinFileKit> descriptor) {
        base.Configure(descriptor);

        descriptor.Field(x => x.VIN).Name("vin");
    }
}



