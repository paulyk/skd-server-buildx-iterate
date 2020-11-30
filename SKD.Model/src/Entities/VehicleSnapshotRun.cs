using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class VehicleSnapshotRun : EntityBase {
        public Guid PlantId { get; set; }
        public Plant Plant { get; set; }
        public DateTime RunDate { get; set; }
        public int Sequence { get; set; }
        public ICollection<VehicleSnapshot> VehicleSnapshots { get; set; } = new List<VehicleSnapshot>();

    }
}