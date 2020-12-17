using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class AssignKitVinInput {
        public string LotNo { get; init; }
        public ICollection<KitVin> Kits { get; set; } = new List<KitVin>();

        public class KitVin {
            public string KitNo { get; init; }
            public string VIN { get; init; }
        }

    }


}