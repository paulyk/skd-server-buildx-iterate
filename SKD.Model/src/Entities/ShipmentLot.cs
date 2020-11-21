using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class ShipmentLot : EntityBase {
        public string LotNo { get; set; }
        public Guid ShipmentId { get; set; }
        public Shipment Shipment { get; set; }
        public ICollection<ShipmentInvoice> Invoices { get; set; } = new List<ShipmentInvoice>();
    }
}