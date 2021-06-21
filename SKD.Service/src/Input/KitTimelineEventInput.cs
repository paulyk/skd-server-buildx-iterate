using SKD.Model;
using System;

namespace SKD.Service {

    public class KitTimelineEventInput {
        public string KitNo { get; init; }
        public TimeLineEventCode EventType { get; init; }
        public DateTime EventDate { get; init; }
        public string EventNote { get; init; }
    }
    
}