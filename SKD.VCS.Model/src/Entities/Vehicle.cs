using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {
    public partial class Vehicle : EntityBase {
        public virtual string VIN { get; set; } = "";
        public string LotNo { get; set; } = "";
        public string KitNo { get; set; } = "";
        public Guid ModelId { get; set; }

        public DateTime? PlannedBuildAt { get; set;}

        public DateTime? ScanLockedAt { get; set; }
        public virtual VehicleModel Model { get; set; }
        public virtual ICollection<VehicleComponent> VehicleComponents { get; set; }

        public Vehicle() : base() {
            VehicleComponents = new List<VehicleComponent>();
        }
    }
}