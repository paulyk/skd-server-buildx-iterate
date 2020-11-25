using System;

namespace SKD.Model {

    public class VehicleSnapshotInput {
        public DateTime RunDate { get; set; }
        public string PlantCode { get; set; }
        public string EngineComponentCode { get; set; }
    }
}