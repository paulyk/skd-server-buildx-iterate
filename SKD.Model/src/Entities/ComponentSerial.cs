using System;
using System.Collections.Generic;

namespace SKD.Model {

    public class ComponentSerial : EntityBase {

        public Guid VehicleComponentId { get; set; }
        public virtual VehicleComponent VehicleComponent { get; set; }
        public string Serial1 { get; set; } = "";
        public string Serial2 { get; set; } = "";

        public DateTime? VerifiedAt { get; set; }

        public ICollection<DcwsResponse> DcwsResponses { get; set; } = new List<DcwsResponse>();

        public ComponentSerial() : base() { }
    }
}