using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class ShipmentPart : EntityBase {
        public Guid PartId { get; set; }
        public Part Part { get; set; }
        public int Quantity { get; set; }

        public Guid ShipmentInvoiceId { get; set; }
        public ShipmentInvoice ShipmentInvoice { get; set; }
    }
}
