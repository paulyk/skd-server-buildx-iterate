using System;

namespace SKD.Model {
    public class LotPartDTO {
        public string LotNo { get; set; }
        public string PartNo { get; set; }
        public string PartDesc { get; set; }
        public int Quantity { get; set; }
        public int QuantityReceived { get; set; }
        public DateTime ImportDate { get; set; }
        public DateTime? ReceivedDate {get; set; }
    }
}