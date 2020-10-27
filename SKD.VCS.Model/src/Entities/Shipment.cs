using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {
    public class Shipment : EntityBase {
        public string SequenceNo { get; set; } = "";
        public ICollection<ShipmentLot> Lots { get; set; } = new List<ShipmentLot>();
    }
}