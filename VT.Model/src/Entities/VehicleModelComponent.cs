using System;

namespace VT.Model {
    public class VehicleModelComponent : EntityBase {

        public Guid VehicleModelId { get; set; }
        public VehicleModel VehicleModel { get; set; }

        public Guid ComponentId { get; set; }
        public Component Component { get; set; }

        public int Quantity { get; set; } = 1;

        public int Sequence { get; set; }
        
        public VehicleModelComponent() : base() {

        }
    }
}