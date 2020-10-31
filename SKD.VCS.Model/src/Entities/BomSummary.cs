using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {
    public class BomSummary : EntityBase {
        public string SequenceNo { get; set; }         
        public Guid ProductionPlantId { get; set; } 
        public ProductionPlant ProductionPlant { get; set; }
        public ICollection<BomSummaryPart> Parts { get; set; } = new List<BomSummaryPart>();
    }
}