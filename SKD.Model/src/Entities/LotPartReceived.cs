namespace SKD.Model;

public partial class LotPartReceived : EntityBase {
    public Guid LotPartId { get; set; }
    public LotPart LotPart { get; set; }
    public int Quantity { get; set; }
}