using System;

namespace SKD.Model {
    public class DCWWResponseInput {
        public Guid ComponentScanId { get; set; }
        public string ResponseCode { get; set; }
        public string ErrorMessage { get;set;}
    }
}