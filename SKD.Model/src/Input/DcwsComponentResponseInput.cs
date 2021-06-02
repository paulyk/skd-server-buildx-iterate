using System;

namespace SKD.Service {
    public class DcwsComponentResponseInput {
        public Guid VehicleComponentId { get; set; }
        public string ResponseCode { get; set; }
        public string ErrorMessage { get;set;}
    }
}