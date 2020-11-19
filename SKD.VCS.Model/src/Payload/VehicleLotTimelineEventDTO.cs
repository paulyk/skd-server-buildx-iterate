using System;

namespace SKD.VCS.Model {
    public class VehicleLotTimelineEventDTO {
        public string LotNo { get; init; }
        public TimeLineEventType EventType { get; init; }
        public DateTime EventDate { get; init; }
        public string EventNote { get; init; }
    }
}