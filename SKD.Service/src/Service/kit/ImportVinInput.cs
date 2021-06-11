using System;
using System.Collections.Generic;

namespace SKD.Common{
    public class ImportVinInput {

        public string PlantCode { get; set; }
        public int Sequence { get; set; }
        public string PartnerPlantCode { get; set; }
        public ICollection<KitVin> Kits { get; set; } = new List<KitVin>();
        public class KitVin {
            public string LotNo { get; set; }
            public string KitNo { get; init; }
            public string VIN { get; init; }
        }
    }


}