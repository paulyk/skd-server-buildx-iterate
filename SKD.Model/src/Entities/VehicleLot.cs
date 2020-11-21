using System;
using System.Collections.Generic;

namespace SKD.Model {
    public partial class VehicleLot : EntityBase {
        public string LotNo { get; set; } = "";
        public Guid PlantId { get; set; }        
        public virtual Plant Plant { get; set; }
        public ICollection<Vehicle> Vehicles { get; set; }  = new List<Vehicle>();
    }
}