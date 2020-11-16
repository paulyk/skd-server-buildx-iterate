using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {
    public class VehicleTimelineEvent : EntityBase {
        public Guid VehicleTimelineEventTypeId { get; set; }
        public VehicleTimelineEventType EventType { get; set;}
        public DateTime EventDate { get; set; }
        public string EventNote { get; set; }

        public Guid VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
    }
}