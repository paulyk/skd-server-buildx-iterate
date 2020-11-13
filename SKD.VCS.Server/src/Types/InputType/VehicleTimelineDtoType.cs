using HotChocolate.Types;
using SKD.VCS.Model;

namespace SKD.VCS.Server {

    // public class TimeLineOptionType : EnumType<TimelineOption> {}

   public class VehicleTimelineDtoType : InputObjectType<VehicleTimelineEventDTO> {
        protected override void Configure(IInputObjectTypeDescriptor<VehicleTimelineEventDTO> descriptor) {
          descriptor.Field(t => t.VIN).Name("vin");          
        }
    }
}