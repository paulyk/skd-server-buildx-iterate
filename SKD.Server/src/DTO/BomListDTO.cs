#nullable enable

using System;
using System.Collections.Generic;

namespace SKD.Server {
    public class BomListDTO {
        public Guid Id { get; set; }
        public string PlantCode { get; set; } = "";
        public int Sequence { get; set; } 
        public int PartCount { get; set; }
        public IEnumerable<BomList_Lot> Lots { get; set; } = new List<BomList_Lot>();
        public DateTime CreatedAt { get; set; }

        public class BomList_Lot {
            public string LotNo { get; set; } = "";
            public int? ShipmentSequence { get; set; }
        }
    }
}