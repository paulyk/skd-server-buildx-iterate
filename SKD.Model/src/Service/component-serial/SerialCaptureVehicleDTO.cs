#nullable enable

using System;
using System.Collections.Generic;

namespace SKD.Model {

    public class SerialCaptureKitDTO {
        public string VIN { get; set; } = "";
        public string KitNo { get; set; } = "";
        public string LotNo { get; set; } = "";
        public string ModelCode { get; set; } = "";
        public string ModelName { get; set; } = "";
        public List<SerialCaptureComponentDTO> KitComponents { get; set; } = new List<SerialCaptureComponentDTO>();
    }

    public class SerialCaptureComponentDTO {
        public Guid KitComponentId { get; set; }
        public int StationSequence { get; set; }
        public string StationCode { get; set; } = "";
        public string StationName { get; set; } = "";
        public string ComponentCode { get; set; } = "";
        public string ComponentName { get; set; } = "";
        public string? Serial1 { get; set; } = "";
        public string? Serial2 { get; set; } = "";
        public DateTime? SerialCapturedAt { get; set; }
    }
}