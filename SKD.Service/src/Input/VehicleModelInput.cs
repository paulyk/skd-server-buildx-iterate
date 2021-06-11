using System;
using System.Collections.Generic;

namespace SKD.Common {
    public class VehicleModelInput {
        public Guid? Id { get; init; }
        public string Code { get; init; }
        public string Name { get; init; }
        public ICollection<ComponentStationInput> ComponentStationInputs { get; set; } = new List<ComponentStationInput>();
    }

    public class ComponentStationInput {
        public string ComponentCode { get; init; }
        public string ProductionStationCode { get; init; }
    }
}