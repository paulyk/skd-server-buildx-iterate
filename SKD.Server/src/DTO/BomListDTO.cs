using System;

namespace SKD.Server {
    public class BomListDTO {
        public Guid Id { get; set; }
        public string PlantCode { get; set; } = "";
        public int Sequence { get; set; } 
        public int LotCount { get; set; }
        public int PartCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}