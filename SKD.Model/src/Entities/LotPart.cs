using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class LotPart : EntityBase {
        public string PartNo { get; set; }
        public string PartDesc { get; set; }
        public int Quantity { get; set; }        

        public Guid LotId { get; set; }
        public VehicleLot Lot { get; set; }
    }
}