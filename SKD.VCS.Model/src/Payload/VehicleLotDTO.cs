using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {

    public class VehicleLotDTO {
        public string LotNo { get; set; }
        public List<Kit> Kits { get; set; }


        public class Kit {
            public string KitNo { get; set; }
            public string ModelCode { get; set; }
        }
    }
}