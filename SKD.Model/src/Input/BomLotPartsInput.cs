using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class BomLotPartsInput {
        public string PlantCode { get; set; }
        public string PartnerCode { get; set; }
        public int Sequence { get; init; }
        public ICollection<LotPartInput> LotParts { get; set; } = new List<LotPartInput>();
    }

    public class LotPartInput {
        public string LotNo { get; init; }
        public string PartNo { get; init; }
        public string PartDesc { get; init; }
        public int Quantity { get; init; }
    }

}