namespace SKD.Server;

public class VehicleTimelineDTOType : ObjectType<VehicleTimelineDTO> {
    protected override void Configure(IObjectTypeDescriptor<VehicleTimelineDTO> descriptor) {
        descriptor.Field(t => t.VIN).Name("vin");
    }
}