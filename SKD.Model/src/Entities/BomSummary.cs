using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class BomSummary : EntityBase {
        public Guid PlantId { get; set; }        
        public Plant Plant { get; set; }
        public int Sequence { get; set; } 
        public bool LotPartQuantitiesMatchShipment { get; set; }
        public ICollection<BomSummaryPart> Parts { get; set; } = new List<BomSummaryPart>();
    }
}