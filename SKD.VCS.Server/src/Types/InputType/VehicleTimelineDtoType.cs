using HotChocolate.Types;
using SKD.VCS.Model;

namespace SKD.VCS.Server {

    // public class TimeLineOptionType : EnumType<TimelineOption> {}

   public class VehicleTimelineDtoType : InputObjectType<VehicleTimelineDTO> {
        protected override void Configure(IInputObjectTypeDescriptor<VehicleTimelineDTO> descriptor) {
          descriptor.Field(t => t.VIN).Name("vin");          
        }
    }
}