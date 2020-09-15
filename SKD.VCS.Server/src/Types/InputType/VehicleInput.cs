using SKD.VCS.Model;
using HotChocolate.Types;
using System;

namespace SKD.VCS.Server {
    public class VehicleInput {
        public string Vin { get; set; } = "";
        public string ModelId { get; set; } = "";
        public string KitNo { get; set; } = "";
        public string LotNo { get; set; } = "";
        public DateTime? PlannedBuildAt { get; set; } = null;
    }
}