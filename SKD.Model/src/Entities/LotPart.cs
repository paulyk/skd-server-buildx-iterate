namespace SKD.Model;

public class LotPart : EntityBase {

    public Guid PartId { get; set; }
    public Part Part { get; set; }
    public int BomQuantity { get; set; }
    public int ShipmentQuantity { get; set; }

    public Guid LotId { get; set; }
    public Lot Lot { get; set; }

    public ICollection<LotPartReceived> Received { get; set; }
}
