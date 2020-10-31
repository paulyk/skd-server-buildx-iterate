using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {
    public class BomDTO {
        public string SequenceNo { get; set; }
        public string ProductionPlantCode { get; set; }
        public ICollection<BomLotDTO> Lots { get; set; } = new List<BomLotDTO>();
    }

    public class BomLotDTO {
        public string LotNo { get; set; }
        public ICollection<BomPartDTO> Parts { get; set; } = new List<BomPartDTO>();
    }

    public class BomPartDTO {
        public string KitNo { get; set; }
        public string PartNo { get; set; }
        public string PartDesc { get; set; }
        public int Quantity { get; set; }
    }

}