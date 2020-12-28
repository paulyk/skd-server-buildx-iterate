using System;

namespace SKD.Model {
    public class LotPartDTO {
        public string LotNo { get; set; }
        public string PartNo { get; set; }
        public string PartDesc { get; set; }
        public int BomQuantity { get; set; }
        public int ShipmentQuantity { get; set; }
        public int QuantityReceived { get; set; }
        public DateTime ImportDate { get; set; }
        public DateTime? ReceivedDate {get; set; }
    }
}