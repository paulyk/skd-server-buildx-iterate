using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class VehicleModelInput {
        public string Code { get; init; }
        public string Name { get; init; }
        public ICollection<ComponeentStationInput> ComponentStationDTOs { get; set; } = new List<ComponeentStationInput>();
    }

    public class ComponeentStationInput {
        public string ComponentCode { get; init; }
        public string ProductionStationCode { get; init; }
    }
}