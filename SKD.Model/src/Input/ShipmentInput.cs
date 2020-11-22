using System;
using System.Collections.Generic;

namespace SKD.Model {


    public class ShipmentDTO {
        public string SequenceNo { get; set; } = "";
        public ICollection<ShipmentLotInput> Lots { get; set; } = new List<ShipmentLotInput>();

    }

    public class ShipmentLotInput {
        public string LotNo { get; set; }
        public ICollection<ShipmentInvoiceInput> Invoices { get; set; } = new List<ShipmentInvoiceInput>();
    }

    public class ShipmentInvoiceInput {
        public string InvoiceNo { get; set; }
        public DateTime ShipDate { get; set; }

        public ICollection<ShipmentPartDTO> Parts { get; set; } = new List<ShipmentPartDTO>();
    }

    public class ShipmentPartDTO {
        public string PartNo { get; set; }
        public string CustomerPartNo { get; set; }
        public string CustomerPartDesc { get; set; }
        public int Quantity { get; set; }

    }
}