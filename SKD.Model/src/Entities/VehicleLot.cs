using System;
using System.Collections.Generic;

namespace SKD.Model {
    public partial class VehicleLot : EntityBase {
        public string LotNo { get; set; } = "";
        
        public Guid PlantId { get; set; }        
        public virtual Plant Plant { get; set; }
        public Guid BomId { get; set; }        
        public virtual Bom Bom { get; set; }

        public ICollection<Vehicle> Vehicles { get; set; }  = new List<Vehicle>();
        public ICollection<LotPart> LotParts { get; set; }  = new List<LotPart>();
    }
}