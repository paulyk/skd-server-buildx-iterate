using System.Collections.Generic;

namespace SKD.Model {
    public class Plant : EntityBase {
        public string Code { get; set; }
        public string Name { get; set; }
        public ICollection<VehicleLot> VehicleLots { get; set; } = new List<VehicleLot>();
        public ICollection<VehicleSnapshotRun> VehicleSnapshotRuns { get; set; } = new List<VehicleSnapshotRun>();
        public ICollection<Bom> Boms { get; set; } = new List<Bom>();
        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    }
}