using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {
    public class ShipmentPart : EntityBase {
        public string PartNo { get; set; }
        public string CustomerPartNo { get; set; }
        public string CustomerPartDesc { get; set; }
        public int Quantity { get; set; }

        public Guid ShipmentInvoiceId { get; set; }
        public ShipmentInvoice ShipmentInvoice { get; set; }
    }
}
