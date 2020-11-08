using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {
    public class VehicleKitDTO {
        public string KitNo { get; set; }
        public string ModelCode { get; set; }
    }   

    public class VehicleLotDTO {
             public string LotNo { get; set; }
             public List<VehicleKitDTO> VehicleDTOs { get; set; }
    }
}