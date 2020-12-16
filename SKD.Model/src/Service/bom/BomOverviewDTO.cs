using System;

namespace SKD.Model {
    public class BomOverviewDTO {
        public Guid Id { get; set; }
        public string PlantCode { get; set; }
        public int Sequence { get; set; }
        public int LotCount { get; set; }
        public int LotPartCount { get; set; }
        public int VehicleCount { get; set; }
        public DateTime CreatedAt {get; set; }
    }
}