using System;
using SKD.Model;

namespace SKD.Server {
    public class TimelineEventDTO {
        public TimeLineEventCode EventType { get; set; } 
        public DateTime? EventDate { get; set; }
        public string? EventNote { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? RemovedAt { get; set; }
        public int Sequence { get; set; }
        
    }
}