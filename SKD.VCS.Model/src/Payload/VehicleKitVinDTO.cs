using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {
    public class VehicleKitVinDTO {
        public string LotNo { get; set; }
        public ICollection<KitVinDTO> Kits { get; set; } = new List<KitVinDTO>();

    }

    public class KitVinDTO {
        public string KitNo { get; set; }
        public string VIN { get; set; }
    }

}