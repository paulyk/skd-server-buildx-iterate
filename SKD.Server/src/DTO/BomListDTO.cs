using System;
using System.Collections.Generic;

namespace SKD.Server {
    public class BomListDTO {
        public Guid Id { get; set; }
        public string PlantCode { get; set; } = "";
        public int Sequence { get; set; } 
        public int PartCount { get; set; }
        public IEnumerable<string> LotNumbers { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
    }
}