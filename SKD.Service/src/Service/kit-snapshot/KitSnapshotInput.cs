using System;

namespace SKD.Model {

    public class KitSnapshotInput {
        /// <summary>
        /// Leave nul to allow system to select current date
        /// </summary>
        public DateTime? RunDate { get; set; }
        public string PlantCode { get; set; }
        public string EngineComponentCode { get; set; }
    }
}