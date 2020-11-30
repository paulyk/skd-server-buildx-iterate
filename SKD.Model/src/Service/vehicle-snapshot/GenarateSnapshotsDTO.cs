using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class GenarateSnapshotDTO {
        public DateTime RunDate { get; set; }
        public int? Sequence { get; set; }
        public string PlantCode { get; set; }
        public int SnapshotCount { get; set; }
        public bool WasGenerated { get; set; }
    }
}