using SKD.Model;
using System;

namespace SKD.Common {

    public class KitTimelineEventInput {
        public string KitNo { get; init; }
        public TimeLineEventType EventType { get; init; }
        public DateTime EventDate { get; init; }
        public string EventNote { get; init; }
    }
    
}