using System;

namespace SKD.VCS.Model {
    public class VehicleDTO {
        public string VIN { get; set; }
        public Guid ModelId { get; set; }
        public string LotNo { get; set; }
        public string KitNo { get; set; }
        public DateTime? PlannedBuildAt { get; set;}
    }

   
}