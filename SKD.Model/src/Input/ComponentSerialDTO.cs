using System;

namespace SKD.Model {
    public class ComponentSerialDTO {
        public Guid ComponentSerialId { get; set; }
        public string VIN { get; set; }
        public string LotNo { get; set; }
        public string ComponentCode { get; set; }
        public string ComponentName { get; set; }
        public string Serial1 { get; set; }
        public string Serial2 { get; set; }

        public DateTime CreatedAt { get; set; }        
    }
}