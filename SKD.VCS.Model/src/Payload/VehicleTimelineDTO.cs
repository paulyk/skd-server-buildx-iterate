using System;

namespace  SKD.VCS.Model
{

    public enum TimelineDateType {
        CustomReceived,
        PlanBuild,
        BuildCompleted,
        GateRelease,
        WholeSate
    
    }
    public class VehicleTimelineDTO {
        public TimelineDateType DateType { get; set; }
        public string VIN {get; set; }
        public DateTime? Date { get; set; }
    }
}