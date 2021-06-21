#nullable enable

using System;

namespace SKD.Service {
    public class PartnerStatusDTO {
        public string PlantCode { get; set; } = "";
        public int Sequence { get; set; }
        public DateTime? RunDate { get; set; } 
        public string ErrorMessage { get; set; } = "";
        public string Filename { get; set; } = "";
        public string PayloadText { get; set; } = "";        
    }
}