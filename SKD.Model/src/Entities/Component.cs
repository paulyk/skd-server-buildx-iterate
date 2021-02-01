using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class Component : EntityBase {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string IconUURL { get; set; } = "";

        public ICollection<VehicleModelComponent> VehicleModelComponents { get; set; }
        public ICollection<KitComponent> KitComponents { get; set; }

        public Component() : base() {
            VehicleModelComponents = new List<VehicleModelComponent>();
            KitComponents = new List<KitComponent>();
        }
    }
}