using System;
using System.Collections.Generic;

namespace SKD.Model {

    public class BomLotKitInput {
        public string PlantCode { get; set; }
        public int Sequence { get; set; }
        public List<Lot> Lots { get; set; }
        public class Lot {
            public string LotNo { get; init; }
            public List<Kit> Kits { get; init; }

            public class Kit {
                public string KitNo { get; init; }
                public string ModelCode { get; init; }
            }
        }
    }
}