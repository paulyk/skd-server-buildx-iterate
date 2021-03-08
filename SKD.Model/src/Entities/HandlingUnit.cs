using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class HandlingUnit : EntityBase {
        public string Code { get; set; }
        public Guid ShipmentInvoiceId { get; set; }
        public ShipmentInvoice Invoice { get; set; }
        public ICollection<ShipmentPart> Parts { get; set; } = new List<ShipmentPart>();
    }
}