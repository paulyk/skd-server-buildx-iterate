using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class KitTimelineEvent : EntityBase {
        public Guid VehicleTimelineEventTypeId { get; set; }
        public KitTimelineEventType EventType { get; set;}
        public DateTime EventDate { get; set; }
        public string EventNote { get; set; }

        public Guid VehicleId { get; set; }
        public Kit Kit { get; set; }
    }
}