using HotChocolate.Types;
using SKD.VCS.Model;

namespace SKD.VCS.Server {

    public class VehicleTimelineEventDTOType : InputObjectType<VehicleTimelineEventDTO> {
        protected override void Configure(IInputObjectTypeDescriptor<VehicleTimelineEventDTO> descriptor) {
            descriptor.Field(t => t.VIN).Name("vin");
        }
    }
}