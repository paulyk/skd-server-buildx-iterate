namespace SKD.Model;

public class Shipment : EntityBase {
    public Guid PlantId { get; set; }
    public Plant Plant { get; set; }
    public int Sequence { get; set; }
    public ICollection<ShipmentLot> ShipmentLots { get; set; } = new List<ShipmentLot>();
}
