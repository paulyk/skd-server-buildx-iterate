using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {
    public partial class Vehicle : EntityBase {
        public virtual string VIN { get; set; } = "";
        public string LotNo { get; set; } = "";
        public string KitNo { get; set; } = "";
        public DateTime? PlannedBuildAt { get; set;}
        public DateTime? ScanLockedAt { get; set; }

        public Guid ModelId { get; set; }        
        public virtual VehicleModel Model { get; set; }
        
        public Guid LotId { get; set; }        
        public virtual VehicleLot Lot { get; set; }

        public virtual ICollection<VehicleComponent> VehicleComponents { get; set; } = new List<VehicleComponent>();
    }
}