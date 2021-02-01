using System.Collections.Generic;

namespace SKD.Model {

    public class ProductionStation : EntityBase {
        public string Code { get; set; }
        public string Name { get; set; }
        public int Sequence { get; set; }
        public ICollection<VehicleModelComponent> ModelComponents { get; set; }
        public ICollection<KitComponent> VehicleComponents { get; set; }

        public ProductionStation() : base() {}
    }
}