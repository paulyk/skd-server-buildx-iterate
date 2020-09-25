using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {
    public class VehicleDTO {
        public string VIN { get; set; }
        public string ModelCode { get; set; }
        public string LotNo { get; set; }
        public string KitNo { get; set; }
        public DateTime? PlannedBuildAt { get; set;}
    }   

    public class VehicleLotDTO {
             public string LotNo { get; set; }
             public List<VehicleDTO> VehicleDTOs { get; set; }
    }
}