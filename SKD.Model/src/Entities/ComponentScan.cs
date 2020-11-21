using System;
using System.Collections.Generic;

namespace SKD.Model {

    public class ComponentScan : EntityBase {

        public Guid VehicleComponentId { get; set; }
        public virtual VehicleComponent VehicleComponent { get; set; }
        public string Scan1 { get; set; } = "";
        public string Scan2 { get; set; } = "";

        public DateTime? AcceptedAt { get; set; }

        public ICollection<DCWSResponse> DCWSResponses { get; set; } = new List<DCWSResponse>();

        public ComponentScan() : base() { }
    }
}