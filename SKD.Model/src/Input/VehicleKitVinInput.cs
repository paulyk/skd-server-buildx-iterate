using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class VehicleKitVinInput {
        public string LotNo { get; init; }
        public ICollection<KitVinInput> Kits { get; set; } = new List<KitVinInput>();
    }

    public class KitVinInput {
        public string KitNo { get; init; }
        public string VIN { get; init; }
    }

}