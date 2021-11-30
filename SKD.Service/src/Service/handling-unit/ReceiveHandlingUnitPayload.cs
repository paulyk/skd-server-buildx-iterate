#nullable enable

namespace SKD.Service;

public class ReceiveHandlingUnitPayload {
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string? InvoiceNo { get; set; }
    public string? LotNo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RemovedAt { get; set; }
}

