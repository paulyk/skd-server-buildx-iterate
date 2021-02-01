using System;
using System.Collections.Generic;

namespace SKD.Model {
    public partial class Kit : EntityBase {
        public virtual string VIN { get; set; } = "";
        public string KitNo { get; set; } = "";

        public Guid ModelId { get; set; }        
        public virtual VehicleModel Model { get; set; }
        
        public Guid LotId { get; set; }        
        public virtual Lot Lot { get; set; }

        public virtual ICollection<KitComponent> KitComponents { get; set; } = new List<KitComponent>();
        public virtual ICollection<VehicleTimelineEvent> TimelineEvents { get; set; } = new List<VehicleTimelineEvent>();
        public virtual ICollection<KitSnapshot> Snapshots { get; set; } = new List<KitSnapshot>();
    }
}