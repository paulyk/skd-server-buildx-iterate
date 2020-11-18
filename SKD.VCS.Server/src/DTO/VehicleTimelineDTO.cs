using System.Collections.Generic;
namespace SKD.VCS.Server {
    public class VehicleTimelineDTO {
        public string VIN { get; set; } = "";
        public string LotNo { get; set; } = "";
        public string KitNo { get; set; } = "";
        public ICollection<TimelineEventDTO> TimelineItems { get; set; } = new List<TimelineEventDTO>();
    }
}