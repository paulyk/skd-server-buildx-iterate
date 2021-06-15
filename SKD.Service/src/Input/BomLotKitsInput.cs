using System;
using System.Collections.Generic;

namespace SKD.Service {

    public class BomLotKitDTO {
        public string PlantCode { get; set; }
        public int Sequence { get; set; }
        public string BomFileCreatedAt { get; set; }
        public List<LotEntry> Lots { get; set; }
        public class LotEntry {
            public string LotNo { get; init; }
            public List<LotKit> Kits { get; init; }

            public class LotKit {
                public string KitNo { get; init; }
                public string ModelCode { get; init; }
            }
        }
    }
}