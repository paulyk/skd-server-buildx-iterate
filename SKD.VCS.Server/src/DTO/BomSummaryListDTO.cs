using System;

namespace SKD.VCS.Server {
    public class BomSummaryListDTO {
        public Guid Id { get; set; }
        public string SequenceNo { get; set; } = "";
        public int PartsCount { get; set; }
        public bool MatchingParts  { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool LotPartQuantitiesMatchShipment  { get; set; }
    }
}