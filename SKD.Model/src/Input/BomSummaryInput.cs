using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class BomSummaryInput {
        public string SequenceNo { get; init; }
        public ICollection<BomSummaryPartInput> Parts { get; set; } = new List<BomSummaryPartInput>();
    }

    public class BomSummaryPartInput {
        public string LotNo { get; init; }
        public string PartNo { get; init; }
        public string PartDesc { get; init; }
        public int Quantity { get; init; }
    }

}