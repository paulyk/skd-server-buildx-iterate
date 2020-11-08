using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {
    public class VehicleModelDTO {
        public string Code { get; set; }
        public string Name { get; set; }
        public ICollection<ComponeentStationDTO> ComponentStationDTOs { get; set; } = new List<ComponeentStationDTO>();
    }

    public class ComponeentStationDTO {
        public string ComponentCode { get; set; }
        public string ProductionStationCode { get; set; }
    }
}