namespace SKD.Server;

public class VehicleTimelineDTOType : ObjectType<KitTimelineDTO> {
    protected override void Configure(IObjectTypeDescriptor<KitTimelineDTO> descriptor) {
        descriptor.Field(t => t.VIN).Name("vin");
    }
}