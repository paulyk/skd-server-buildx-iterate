using System;

namespace SKD.Service {
    public class ShipmentOverviewDTO {
        public Guid Id { get; set; }
        public string PlantCode { get; set; }
        public Guid BomId { get; set; }
        public int BomSequence { get; set; }
        public int Sequence { get; set; }
        public int LotCount { get; set; }
        public int InvoiceCount { get; set; }
        
        public int HandlingUnitCount { get; set; }
        public int HandlingUnitReceivedCount { get; set; }

        public int LotPartCount { get; set; }
        public int LotPartReceivedCount { get; set; }

        public int BomShipDiffCount { get; set; }

        public int LotPartReceiveBomDiffCount { get; set; }

        public ICollection<string> LotNumbers { get; set;}  = new List<string>();

        public int PartCount { get; set; }
        public DateTime CreatedAt {get; set; }
    }
}