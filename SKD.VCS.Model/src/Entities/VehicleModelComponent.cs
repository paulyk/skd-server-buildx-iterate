using System;

namespace SKD.VCS.Model {
    public class VehicleModelComponent : EntityBase {

        public Guid VehicleModelId { get; set; }
        public VehicleModel VehicleModel { get; set; }

        public Guid ComponentId { get; set; }
        public Component Component { get; set; }

        public Guid ProductionStationId { get; set; }
        public ProductionStation ProductionStation { get; set; }

        public VehicleModelComponent() : base() {}
    }
}