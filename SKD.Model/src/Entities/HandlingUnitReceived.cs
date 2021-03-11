using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class HandlingUnitReceived : EntityBase {
        public Guid HandlingUnitId { get; set; }
        public HandlingUnit HandlingUnit { get; set; }
    }
}