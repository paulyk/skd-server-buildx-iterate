using System.Collections.Generic;

namespace SKD.Model {
    public class Plant : EntityBase {
        public string Code { get; set; }
        public string Name { get; set; }
        public ICollection<BomSummary> BomSummaries { get; set; } = new List<BomSummary>();
        public ICollection<VehicleLot> VehicleLots { get; set; } = new List<VehicleLot>();
        public ICollection<VehicleSnapshotRun> VehicleSnapshotRuns { get; set; } = new List<VehicleSnapshotRun>();
    }
}