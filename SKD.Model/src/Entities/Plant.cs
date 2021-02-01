using System.Collections.Generic;

namespace SKD.Model {
    public class Plant : EntityBase {
        public string Code { get; set; }
        public string Name { get; set; }
        public ICollection<Lot> Lots { get; set; } = new List<Lot>();
        public ICollection<kitSnapshotRun> VehicleSnapshotRuns { get; set; } = new List<kitSnapshotRun>();
        public ICollection<Bom> Boms { get; set; } = new List<Bom>();
        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    }
}