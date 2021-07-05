using System;
using System.Collections.Generic;

namespace SKD.Model {
    public partial class VehicleModel : EntityBase {
        public string Code { get; set; } 
        public string Description { get; set; }
        public string ModelYear { get; set; }
        public string Model { get; set; }
        public string Series { get; set; }
        public string Body { get; set; }
        public ICollection<Lot> Lots { get; set; }
        public ICollection<VehicleModelComponent> ModelComponents { get; set; }

        public VehicleModel() : base() {
            ModelComponents = new List<VehicleModelComponent>();
        }
    }
}