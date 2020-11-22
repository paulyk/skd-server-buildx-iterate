using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class VehicleKitVinDTO {
        public string LotNo { get; init; }
        public ICollection<KitVinDTO> Kits { get; set; } = new List<KitVinDTO>();
    }

    public class KitVinDTO {
        public string KitNo { get; init; }
        public string VIN { get; init; }
    }

}