#nullable enable
using System;
using System.Collections.Generic;

namespace SKD.Service {


    public class ShipFile {
        public string PlantCode { get; set; } = "";
        public int Sequence { get; set; } 
        public ICollection<ShipFileLot> Lots { get; set; } = new List<ShipFileLot>();
    }

    public class ShipFileLot {
        public string LotNo { get; set; } = "";
        public ICollection<ShipFileInvoice> Invoices { get; set; } = new List<ShipFileInvoice>();
    }

    public class ShipFileInvoice {
        public string InvoiceNo { get; set; } = "";
        public DateTime ShipDate { get; set; }

        public ICollection<ShipFilePart> Parts { get; set; } = new List<ShipFilePart>();
    }

    public class ShipFilePart {
        public string PartNo { get; set; } = "";
        public string HandlingUnitCode { get; set; } = "";
        public string CustomerPartNo { get; set; } = "";
        public string CustomerPartDesc { get; set; } = "";
        public int Quantity { get; set; }
    }
}