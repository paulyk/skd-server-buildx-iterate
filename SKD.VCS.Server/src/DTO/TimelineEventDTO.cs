using System;

namespace SKD.VCS.Server {
    public class TimelineEventDTO {
        public string EventType { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string? EventNote { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RemovedAt { get; set; }
        
    }
}