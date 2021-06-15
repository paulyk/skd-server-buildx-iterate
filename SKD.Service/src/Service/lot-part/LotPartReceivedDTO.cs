using System;

namespace SKD.Service {
    public class LotParReceivedtDTO {
        public string LotNo { get; set; }
        public string PartNo { get; set; }
        public string PartDesc { get; set; }
        public int ReceivedQuantity { get; set; }
        public int BomQuantity { get; set; }
        public int ShipmentQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RemovedAt { get; set; }
    }
}