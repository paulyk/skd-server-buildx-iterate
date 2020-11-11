using System;

namespace SKD.VCS.Model {

    public enum TimelineOption {
        CUSTOM_RECEIVED,
        PLAN_BUILD,
        BUILD_COMPLETED,
        GATE_RELEASE,
        WHOLESALE
    }

 
    public class VehicleTimelineDTO {
        public TimelineOption DateType { get; set; }
        public string VIN { get; set; }
        public DateTime? Date { get; set; }
    }
}