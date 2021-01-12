namespace SKD.Dcws {
    public class CdcComponentResponse {
        public string VIN { get; set; }
        public string ComponentTypeCode { get; set; }
        public string Serial1 { get; set; }
        public string Serial2 { get; set; }
        public bool Success { get; set; }
        public string ProcessException { get; set; }
    }
}