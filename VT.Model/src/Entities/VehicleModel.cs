using System;
using System.Collections.Generic;

namespace VT.Model {
    public class VehicleModel : EntityBase {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public ICollection<Vehicle> Vehicles { get; set; }
        public ICollection<VehicleModelComponent> ComponentMappings { get; set; }

        public VehicleModel() : base() {
            Vehicles = new List<Vehicle>();
            ComponentMappings = new List<VehicleModelComponent>();
        }
    }
}