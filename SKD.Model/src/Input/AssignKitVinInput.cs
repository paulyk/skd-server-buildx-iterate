using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class AssignKitVinInput {
        public ICollection<KitVin> Kits { get; set; } = new List<KitVin>();

        public class KitVin {
            public string LotNo { get; set; }
            public string KitNo { get; init; }
            public string VIN { get; init; }
        }

    }


}