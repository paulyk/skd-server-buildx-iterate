using System;
using System.Collections.Generic;

namespace SKD.Service{
    public class VinFile {

        public string PlantCode { get; set; }
        public int Sequence { get; set; }
        public string PartnerPlantCode { get; set; }
        public ICollection<VinFileKit> Kits { get; set; } = new List<VinFileKit>();
        public class VinFileKit {
            public string LotNo { get; set; }
            public string KitNo { get; init; }
            public string VIN { get; init; }
        }
    }

}