using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {
    public class BomPart : EntityBase {
        public string KitNo { get; set; }
        public string PartNo { get; set; }         
        public string PartDesc { get; set; }
        public int Quantity { get; set; }

        public Guid BomLotId { get; set; }
        public BomLot BomLot { get; set; }
    }
}