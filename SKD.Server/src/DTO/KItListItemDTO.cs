using System;
using SKD.Model;

namespace SKD.Server {
    public class KitListItemDTO {
        public   Guid Id { get; set; }
        public string LotNo { get; set; } = "";
        public string KitNo { get; set; } = "";
        public string VIN { get; set; } = "";
        public string ModelCode { get; set; } = "";
        public string ModelName { get; set; } = "";
        public string? LastTimelineEvent { get; set; } = "";
        public int? ComponentCount { get; set; }
        public int? ScannedComponentCount { get; set; }
        public int? VerifiedComponentCount { get; set; }
        public DateTime? Imported  { get; set; }
    }        
}