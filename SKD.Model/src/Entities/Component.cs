using System;
using System.Collections.Generic;

namespace SKD.Model {

public enum SerialCaptureRequirement {
        NOT_REQUIRED,
        REQUIRED,
        SERIAL_1_ONLY,
        SERIAL_1_AND_2
    }    
    public class Component : EntityBase {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string IconUURL { get; set; } = "";
        public SerialCaptureRequirement SerialCaptureRequirement { get; set; }

        public ICollection<VehicleModelComponent> VehicleModelComponents { get; set; }
        public ICollection<KitComponent> KitComponents { get; set; }

        public Component() : base() {
            VehicleModelComponents = new List<VehicleModelComponent>();
            KitComponents = new List<KitComponent>();
        }
    }
}