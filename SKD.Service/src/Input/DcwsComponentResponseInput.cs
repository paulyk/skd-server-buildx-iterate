using System;

namespace SKD.Common {
    public class DcwsComponentResponseInput {
        public Guid VehicleComponentId { get; set; }
        public string ResponseCode { get; set; }
        public string ErrorMessage { get;set;}
    }
}