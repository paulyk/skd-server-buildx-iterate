using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class Part : EntityBase {
        public string PartNo { get; set; }
        public string PartDesc { get; set; }
        public ICollection<LotPart> LotParts { get; set; } = new List<LotPart>();
        public ICollection<ShipmentPart> ShipmentParts { get; set; } = new List<ShipmentPart>();
    }
}