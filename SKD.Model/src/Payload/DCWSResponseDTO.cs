using System;

namespace SKD.Model {
    public class DCWWResponseDTO {
        public Guid ComponentScanId { get; set; }
        public string ResponseCode { get; set; }
        public string ErrorMessage { get;set;}
    }
}