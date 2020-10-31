using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {
    public class BomLot : EntityBase {
        public string LotNo { get; set; }         
        public Guid BomId { get; set; }
        public Bom Bom { get; set; }
        public ICollection<BomPart> Parts { get; set; } = new List<BomPart>();
    }
}