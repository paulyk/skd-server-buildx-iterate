using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class Shipment : EntityBase {
        public Guid PlantId { get; set; }
        public Plant Plant { get; set; }
        public int Sequence { get; set; } 
        public ICollection<ShipmentLot> Lots { get; set; } = new List<ShipmentLot>();
    }
}