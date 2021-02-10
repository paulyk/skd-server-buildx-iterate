using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class BomLotPartInput {
        public string PlantCode { get; set; }
        public int Sequence { get; init; }
        public ICollection<LotPart> LotParts { get; set; } = new List<LotPart>();

        public class LotPart {
            public string LotNo { get; init; }
            public string PartNo { get; init; }
            public string PartDesc { get; init; }
            public int Quantity { get; set; }
        }        
    }
}