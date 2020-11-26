using System;
using System.Collections.Generic;

namespace SKD.Model {
    public partial class Vehicle : EntityBase {
        public virtual string VIN { get; set; } = "";
        public string KitNo { get; set; } = "";

        public Guid ModelId { get; set; }        
        public virtual VehicleModel Model { get; set; }
        
        public Guid LotId { get; set; }        
        public virtual VehicleLot Lot { get; set; }

        public virtual ICollection<VehicleComponent> VehicleComponents { get; set; } = new List<VehicleComponent>();
        public virtual ICollection<VehicleTimelineEvent> TimelineEvents { get; set; } = new List<VehicleTimelineEvent>();
        public virtual ICollection<VehicleSnapshot> Snapshots { get; set; } = new List<VehicleSnapshot>();
    }
}