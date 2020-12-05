using System;

namespace SKD.Server {
    public class BomSummaryListDTO {
        public Guid Id { get; set; }
        public string PlantCode { get; set; } = "";
        public int Sequence { get; set; } 
        public int PartsCount { get; set; }
        public bool MatchingParts  { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool LotPartQuantitiesMatchShipment  { get; set; }
    }
}