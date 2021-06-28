#nullable enable
using System;
using System.Collections.Generic;

namespace SKD.Service {
    public class VehicleModelInput {
        public Guid? Id { get; init; }
        public string Code { get; init; } = "";
        public string Name { get; init; } = "";
        public ICollection<ComponentStationInput> ComponentStationInputs { get; set; } = new List<ComponentStationInput>();
    }

    public class ComponentStationInput {
        public string ComponentCode { get; init; } = "";
        public string ProductionStationCode { get; init; } = "";
    }

    public class VehicleModelFromExistingInput {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string ExistingModelCode { get; set; } = "";
    }
}