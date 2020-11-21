using System;

namespace SKD.Model {

    public class VehicleTimelineEventDTO {
        public string KitNo { get; init; }
        public TimeLineEventType EventType { get; init; }
        public DateTime EventDate { get; init; }
        public string EventNote { get; init; }
    }
    
}