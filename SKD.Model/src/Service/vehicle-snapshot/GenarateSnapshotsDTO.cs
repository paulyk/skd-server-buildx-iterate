using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class GenarateSnapshotsDTO {
        public DateTime RunDate { get; set; }
        public string PlantCode { get; set; }
        public int SnapshotCount { get; set; }
    }
}