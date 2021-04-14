using System;
using System.Collections.Generic;

namespace SKD.Model {

public enum DcwsSerialCaptureRule {
        UNKNOWN,
        SERIAL_1_ONLY,
        SERIAL_1_AND_2,
        NOT_REQUIED,
    }    
    public class Component : EntityBase {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string IconUURL { get; set; } = "";
        public DcwsSerialCaptureRule DcwsSerialCaptureRule { get; set; }

        public ICollection<VehicleModelComponent> VehicleModelComponents { get; set; }
        public ICollection<KitComponent> KitComponents { get; set; }

        public Component() : base() {
            VehicleModelComponents = new List<VehicleModelComponent>();
            KitComponents = new List<KitComponent>();
        }
    }
}