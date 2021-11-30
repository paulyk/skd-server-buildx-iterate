#nullable enable

namespace SKD.Service;

public class ReceiveLotPartInput {
    public string LotNo { get; set; } = "";
    public string PartNo { get; set; } = "";
    public int Quantity { get; set; }
}
