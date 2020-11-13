using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {
    public class VehicleModelDTO {
        public string Code { get; init; }
        public string Name { get; init; }
        public ICollection<ComponeentStationDTO> ComponentStationDTOs { get; set; } = new List<ComponeentStationDTO>();
    }

    public class ComponeentStationDTO {
        public string ComponentCode { get; init; }
        public string ProductionStationCode { get; init; }
    }
}