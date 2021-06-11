using SKD.Model;
using System;

namespace SKD.Common {
    public class LotTimelineEventInput {
        public string LotNo { get; init; }
        public TimeLineEventType EventType { get; init; }
        public DateTime EventDate { get; init; }
        public string EventNote { get; init; }
    }
}