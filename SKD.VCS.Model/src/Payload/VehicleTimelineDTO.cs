using System;

namespace  SKD.VCS.Model
{
    public class VehicleTimelineDTO {
        public string VIN {get; set; }
        public DateTime? CustomReceivedAt { get; set; }
        public DateTime? PlanBuildAt { get; set; }
        public DateTime? BuildCompletedAt { get; set; }
        public DateTime? GateRleaseAt { get; set; }
        public DateTime? WholeStateAt { get; set; }
    }
}