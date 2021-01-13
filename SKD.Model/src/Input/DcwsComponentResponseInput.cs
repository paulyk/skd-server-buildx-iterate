using System;

namespace SKD.Model {
    public class DcwsComponentResponseInput {
        public Guid VehicleComponentId { get; set; }
        public string ResponseCode { get; set; }
        public string ErrorMessage { get;set;}
    }
}