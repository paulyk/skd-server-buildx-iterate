using System;

namespace SKD.VCS.Model {
    public class DCWWResponseDTO {
        public Guid ComponentScanId { get; set; }
        public string ResponseCode { get; set; }
        public string ErrorMessage { get;set;}
        public bool Accepted { get; set; }
    }
}