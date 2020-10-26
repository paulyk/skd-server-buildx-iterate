
using System;
using System.Collections.Generic;


namespace SKD.VCS.Model {
    public class ShipmentInvoice : EntityBase {
        public string InnvoiceNo { get; set; }
        public DateTime ShipDate { get; set; }

        public Guid ShipmentLotId { get; set; }
        public ShipmentLot ShipmentLot { get; set; }
        public ICollection<ShipmentPart> Parts { get; set; } = new List<ShipmentPart>();
    }
}