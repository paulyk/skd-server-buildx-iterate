using System;

namespace SKD.Model {
    public class VehicleComponent : EntityBase {

        public Guid VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }


        public Guid ComponentId { get; set; }
        public Component Component { get; set; }


        public int Sequence { get; set; }
        public string Scan1 { get; set; }
        public string Scan2 { get; set; }
        
        public DateTime? ScanAt { get; set; }

        public VehicleComponent() {

        }
    }
}