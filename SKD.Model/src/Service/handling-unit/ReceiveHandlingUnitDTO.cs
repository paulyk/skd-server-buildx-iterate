#nullable enable

using System;

namespace SKD.Service{
    public class ValidateReceiveHandlingUnitPayload {
        public string? Code { get; set; }
        public string? InvoiceNo { get; set; }
        public string? LotNo { get; set; }
        public string? ModelCode  { get; set; }
        public string? ModelName { get; set; }
        public int PartCount { get; set; }
        public DateTime? ReceivedAt { get; set; }
    }
}
