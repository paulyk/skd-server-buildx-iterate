using System;
using System.Collections.Generic;

namespace SKD.Model {

    public class VehicleLotInput {
        public string LotNo { get; init; }
        public string PlantCode { get; set; }
        public List<Kit> Kits { get; init; }

        public class Kit {
            public string KitNo { get; init; }
            public string ModelCode { get; init; }
        }
    }
}