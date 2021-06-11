using System;
using System.Collections.Generic;

namespace SKD.Common {


    public class ShipmentInput {
        public string PlantCode { get; set; }
        public int Sequence { get; set; } 
        public ICollection<ShipmentLotInput> Lots { get; set; } = new List<ShipmentLotInput>();
    }

    public class ShipmentLotInput {
        public string LotNo { get; set; }
        public ICollection<ShipmentInvoiceInput> Invoices { get; set; } = new List<ShipmentInvoiceInput>();
    }

    public class ShipmentInvoiceInput {
        public string InvoiceNo { get; set; }
        public DateTime ShipDate { get; set; }

        public ICollection<ShipmentPartInput> Parts { get; set; } = new List<ShipmentPartInput>();
    }

    public class ShipmentPartInput {
        public string PartNo { get; set; }
        public string HandlingUnitCode { get; set; }
        public string CustomerPartNo { get; set; }
        public string CustomerPartDesc { get; set; }
        public int Quantity { get; set; }
    }
}