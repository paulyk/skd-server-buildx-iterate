using System;
using System.Collections.Generic;

namespace SKD.Server {
    public class PartQuantityDTO {
        public string PartNo { get; set; } = "";
        public string PartDesc { get; set; } = "";
        public int Quantity { get; set; }
    }
}