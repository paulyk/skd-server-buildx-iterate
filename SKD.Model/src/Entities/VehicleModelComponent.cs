using System;

namespace SKD.Model {
    public class VehicleModelComponent : EntityBase {

        public Guid VehicleModelId { get; set; }
        public VehicleModel VehicleModel { get; set; }

        public Guid ComponentId { get; set; }
        public Component Component { get; set; }
        public int Sequence { get; set; }
        public string PrerequisiteSequences { get; set; }
        
        public VehicleModelComponent() : base() {

        }
    }
}