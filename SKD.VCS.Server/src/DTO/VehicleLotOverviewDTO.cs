using System;

namespace SKD.VCS.Server {
    public class VehicleLotOverviewDTO {
        public Guid Id { get; set; }
        public string LotNo { get; set; } = "";
        public string ModelCode { get; set; } = "";
        public string ModelName { get; set; } = "";
        public TimelineEventDTO? CustomReceived { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}