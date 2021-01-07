using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class VehicleComponent : EntityBase {

        public Guid VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }

        public Guid ComponentId { get; set; }
        public Component Component { get; set; }

        public Guid ProductionStationId { get; set; }
        public ProductionStation ProductionStation { get; set; }

        public virtual ICollection<ComponentSerial> ComponentSerials { get; set; } = new List<ComponentSerial>();
        public DateTime? ScanVerifiedAt { get; set; }

        public VehicleComponent(): base() {

        }
    }
}