using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {
    public class BomSummaryDTO {
        public string SequenceNo { get; set; }
        public ICollection<BomSummaryPartDTO> Parts { get; set; } = new List<BomSummaryPartDTO>();
    }


    public class BomSummaryPartDTO {
        public string LotNo { get; set; }
        public string PartNo { get; set; }
        public string PartDesc { get; set; }
        public int Quantity { get; set; }
    }

}