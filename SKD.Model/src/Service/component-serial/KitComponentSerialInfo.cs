#nullable enable

using System;
using System.Collections.Generic;

namespace SKD.Model {

    public class KitComponentSerialInfo {
        public string ComponentCode { get; set; } = "";
        public string ComponentName { get; set; } = "";    
        public ICollection<StatcionSerialInfo> Stations { get; set; } = new List<StatcionSerialInfo>();
    }

    public class StatcionSerialInfo {
        public Guid KitComponentId { get; set; }
        public int StationSequence { get; set; } 
        public string StationCode { get; set; } = "";
        public string StationName { get; set; } = "";        
        public string? Serial1 { get; set; } = "";        
        public string? Serial2 { get; set; } = "";
        public DateTime? CreatedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }

    }

}