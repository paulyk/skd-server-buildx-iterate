using System.Collections.Generic;

namespace SKD.Model {

    public class ProductionStation : EntityBase {
        public string Code { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
        public ICollection<VehicleModelComponent> ModelComponents { get; set; }
        public ICollection<VehicleComponent> VehicleComponents { get; set; }

        public ProductionStation() : base() {}
    }
}