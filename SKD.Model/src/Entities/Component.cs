using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class Component : EntityBase {
        public string Code { get; set; }
        public string Name { get; set; }

        public ICollection<VehicleModelComponent> VehicleModelComponents { get; set; }
        public ICollection<VehicleComponent> VehicleComponents { get; set; }

        public Component() : base() {
            VehicleModelComponents = new List<VehicleModelComponent>();
            VehicleComponents = new List<VehicleComponent>();
        }
    }
}