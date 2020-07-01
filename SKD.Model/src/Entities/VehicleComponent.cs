using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class VehicleComponent : EntityBase {

        public Guid VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }


        public Guid ComponentId { get; set; }
        public Component Component { get; set; }
        public int Sequence { get; set; }
        public virtual ICollection<VehicleComponentScan> ComponentScans { get; set; } = new List<VehicleComponentScan>();

        public VehicleComponent() {

        }
    }
}