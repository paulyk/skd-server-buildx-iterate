using System;

namespace SKD.Model {
    public class LotTimelineEventInput {
        public string LotNo { get; init; }
        public TimeLineEventType EventType { get; init; }
        public DateTime EventDate { get; init; }
        public string EventNote { get; init; }
    }
}