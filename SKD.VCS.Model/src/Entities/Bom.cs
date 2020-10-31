using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {
    public class Bom : EntityBase {
        public string SequenceNo { get; set; }         
        public Guid ProductionPlantId { get; set; } 
        public ProductionPlant ProductionPlant { get; set; }
        public ICollection<BomLot> Lots { get; set; } = new List<BomLot>();
    }
}