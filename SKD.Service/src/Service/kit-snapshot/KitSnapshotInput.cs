#nullable enable
using System;

namespace SKD.Model {

    public class KitSnapshotInput {
        /// <summary>
        /// Leave null to allow system to select current date
        /// </summary>
        public DateTime? RunDate { get; set; }
        public string PlantCode { get; set; } = "";
        public string EngineComponentCode { get; set; } = "";
        public bool RejectIfNoChanges { get; set; } = true;
    }
}