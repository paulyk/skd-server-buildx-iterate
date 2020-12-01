using System;
using System.Collections.Generic;

namespace SKD.Model {
    public partial class VehicleModel : EntityBase {
        public string Code { get; set; } 
        public string Name { get; set; }
        public string Type { get; set; }
        public ICollection<Vehicle> Vehicles { get; set; }
        public ICollection<VehicleModelComponent> ModelComponents { get; set; }

        public VehicleModel() : base() {
            Vehicles = new List<Vehicle>();
            ModelComponents = new List<VehicleModelComponent>();
        }
    }
}