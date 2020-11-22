using System;

namespace SKD.Model {
    public class VehicleLotTimelineEventInput {
        public string LotNo { get; init; }
        public TimeLineEventType EventType { get; init; }
        public DateTime EventDate { get; init; }
        public string EventNote { get; init; }
    }
}