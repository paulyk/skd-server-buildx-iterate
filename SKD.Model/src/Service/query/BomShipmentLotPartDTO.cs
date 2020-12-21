using System;

namespace SKD.Model {
    public class BomShipmentLotPartDTO {
        public string PlantCode { get; set; }
        public int BomSequence { get; set; }
        public string LotNo { get; set; }
        public string PartNo { get; set; }
        public string PartDesc { get; set; }
        public int BomQuantity { get; set; }
        public int ShipmentQuantity { get; set; }
    }
}