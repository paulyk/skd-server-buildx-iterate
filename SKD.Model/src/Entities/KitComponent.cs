using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class KitComponent : EntityBase {

        public Guid KitId { get; set; }
        public Kit Kit { get; set; }

        public Guid ComponentId { get; set; }
        public Component Component { get; set; }

        public Guid ProductionStationId { get; set; }
        public ProductionStation ProductionStation { get; set; }

        public virtual ICollection<ComponentSerial> ComponentSerials { get; set; } = new List<ComponentSerial>();
        public DateTime? VerifiedAt { get; set; }

        public KitComponent(): base() {

        }
    }
}