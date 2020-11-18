using HotChocolate.Types;
using SKD.VCS.Model;

namespace SKD.VCS.Server {

    public class VehicleTimelineDTOType : ObjectType<VehicleTimelineDTO> {
        protected override void Configure(IObjectTypeDescriptor<VehicleTimelineDTO> descriptor) {
            descriptor.Field(t => t.VIN).Name("vin");
        }
    }
}