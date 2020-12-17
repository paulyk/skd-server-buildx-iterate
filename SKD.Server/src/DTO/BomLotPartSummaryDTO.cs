using System;
using System.Collections.Generic;

namespace SKD.Server {
    public class BomLotPartSummaryDTO {
        public Guid Id { get; set; }
        public string PlantCode { get; set; } = "";
        public int Sequence { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<LotPartSummary> Parts { get; set; } = new List<LotPartSummary>();

        public class LotPartSummary {
            public string LotNo { get; set; } = "";
            public string PartNo { get; set; } = "";
            public string PartDesc { get; set; } = "";
            public int Quantity { get; set; }

        }
    }
}