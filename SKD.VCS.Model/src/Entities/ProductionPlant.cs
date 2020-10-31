using System.Collections.Generic;

namespace SKD.VCS.Model {
    public class ProductionPlant : EntityBase { 
        public string Code { get; set; }
        public string Name { get; set; }
        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
        public ICollection<Bom> Boms { get; set; } = new List<Bom>();
    }
}