using System;

namespace SKD.Model {
    public class BomShipmentLotPartDTO {
        public string LotNo { get; set; }
        public string PartNo { get; set; }
        public string PartDesc { get; set; }
        public int BomQuantity { get; set; }
        public int ShipmentQuantity { get; set; }
    }
}