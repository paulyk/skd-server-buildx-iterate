using System;
using System.Collections.Generic;

namespace VT.Model {
    public class Vehicle : EntityBase {
        public string VIN { get; set; }
        public string KitNo { get; set; }
        public string LotNo { get; set; }
        public Guid ModelId { get; set; }
        public VehicleModel Model { get; set; }
        public ICollection<VehicleComponent> VehicleComponents { get; set; }

        public Vehicle() : base() {
            VehicleComponents = new List<VehicleComponent>();
        }
    }
}