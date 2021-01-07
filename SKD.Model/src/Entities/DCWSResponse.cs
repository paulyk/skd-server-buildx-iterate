using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class DCWSResponse : EntityBase {
        public string ResponseCode { get; set; } = "";
        public string ErrorMessage { get; set; }

        public Guid ComponentScanId { get; set;}
        public ComponentSerial ComponentSerial { get; set; }

        public bool DcwsSuccessfulSave { get; set; }
    }
}