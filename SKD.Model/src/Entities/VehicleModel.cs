using System;
using System.Collections.Generic;

namespace SKD.Model {
    public partial class VehicleModel : EntityBase {
        public string Code { get; set; } 
        public string Name { get; set; }
        public string Type { get; set; }
        public ICollection<Lot> Lots { get; set; }
        public ICollection<VehicleModelComponent> ModelComponents { get; set; }

        public VehicleModel() : base() {
            ModelComponents = new List<VehicleModelComponent>();
        }
    }
}