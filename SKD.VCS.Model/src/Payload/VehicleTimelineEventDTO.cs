using System;

namespace SKD.VCS.Model {

    public class VehicleTimelineEventDTO {
        public string VIN { get; init; }
        public string EventTypeCode { get; init; }
        public DateTime EventDate { get; init; }
    }
    
}