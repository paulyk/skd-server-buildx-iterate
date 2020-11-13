using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {

    public class VehicleLotDTO {
        public string LotNo { get; init; }
        public List<Kit> Kits { get; init; }


        public class Kit {
            public string KitNo { get; init; }
            public string ModelCode { get; init; }
        }
    }
}