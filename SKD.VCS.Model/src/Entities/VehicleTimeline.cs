using System;

namespace SKD.VCS.Model {
    public class VehicleTimeline : EntityBase {

        public DateTime? CustomReceivedAt { get; set; }
        public DateTime? PlanBuildAt { get; set; }
        public DateTime? BuildCompletedAt { get; set; }
        public DateTime? GateRleaseAt { get; set; }
        public DateTime? WholeStateAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        public Guid VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
    }
}