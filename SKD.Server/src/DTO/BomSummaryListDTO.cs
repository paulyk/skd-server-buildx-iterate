using System;

namespace SKD.Server {
    public class BomSummaryListDTO {
        public Guid Id { get; set; }
        public string PlantCode { get; set; } = "";
        public int Sequence { get; set; } 
        public int LotCount { get; set; }
        public int LotPartCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}