using System;

namespace SKD.Service;

public class HandlingUnitOverview {
    public string PlantCode { get; set; }
    public int ShipmentSequence { get; set; }
    public string HandlingUnitCode { get; set; }
    public string LotNo { get; set; }
    public string InvoiceNo { get; set; }
    public int PartCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public DateTime? ReceiveCancledAt { get; set; }
}