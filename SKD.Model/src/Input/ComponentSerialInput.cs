using System;

namespace SKD.Model {
    public class ComponentSerialInput {
        public Guid VehicleComponentId { get; set; }
        public string Serial1 { get; set; } = "";
        public string Serial2 { get; set; } = "";
        public Boolean Replace { get; set; } 
    }
}