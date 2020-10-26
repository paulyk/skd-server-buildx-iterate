using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {


    public class ShipmentDTO {
        public string ShipSequenceNo { get; set; } = "";
        public ICollection<ShipmentLotDTO> Lots { get; set; } = new List<ShipmentLotDTO>();

    }

    public class ShipmentLotDTO {
        public string LotNo { get; set; }
        public ICollection<ShipmentInvoiceDTO> Invoices { get; set; } = new List<ShipmentInvoiceDTO>();
    }

    public class ShipmentInvoiceDTO {
        public string InnvoiceNo { get; set; }
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