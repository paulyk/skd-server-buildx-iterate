using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {
    public class BomSummaryDTO {
        public string SequenceNo { get; init; }
        public ICollection<BomSummaryPartDTO> Parts { get; set; } = new List<BomSummaryPartDTO>();
    }

    public class BomSummaryPartDTO {
        public string LotNo { get; init; }
        public string PartNo { get; init; }
        public string PartDesc { get; init; }
        public int Quantity { get; init; }
    }

}