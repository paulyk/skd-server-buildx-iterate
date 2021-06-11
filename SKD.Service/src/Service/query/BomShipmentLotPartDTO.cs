using System;

namespace SKD.Common {
    public class BomShipmentLotPartDTO {
        public string LotNo { get; set; }
        public string PartNo { get; set; }
        public string PartDesc { get; set; }
        public int BomQuantity { get; set; }
        public int ShipmentQuantity { get; set; }
    }
}