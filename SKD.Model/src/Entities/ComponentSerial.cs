using System;
using System.Collections.Generic;

namespace SKD.Model {

    public class ComponentSerial : EntityBase {

        public Guid VehicleComponentId { get; set; }
        public virtual VehicleComponent VehicleComponent { get; set; }
        public string Serial1 { get; set; } = "";
        public string Serial2 { get; set; } = "";

        public DateTime? AcceptedAt { get; set; }

        public ICollection<DCWSResponse> DCWSResponses { get; set; } = new List<DCWSResponse>();

        public ComponentSerial() : base() { }
    }
}