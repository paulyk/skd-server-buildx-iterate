using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {
    public class BomSummaryPart : EntityBase {
        public string LotNo { get; set; }
        public string PartNo { get; set; }
        public string PartDesc { get; set; }
        public int Quantity { get; set; }
        public bool MatcheShipmentLotPartQuantity { get; set; }
        public Guid BomSummaryId { get; set; }
        public BomSummary BomSummary { get; set; }
    }
}