using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {
    public class DCWSResponse : EntityBase {
        public string ResponseCode { get; set; } = "";
        public string ErrorMessage { get; set; }

        public Guid ComponentScanId { get; set;}
        public ComponentScan ComponentScan { get; set; }

        public DateTime? AcceptedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
    }
}