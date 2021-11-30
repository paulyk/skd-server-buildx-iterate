namespace SKD.Model;

public class ShipmentPart : EntityBase {
    public Guid PartId { get; set; }
    public Part Part { get; set; }
    public int Quantity { get; set; }
    public Guid? HandlingUnitId { get; set; }
    public HandlingUnit HandlingUnit { get; set; }
}

