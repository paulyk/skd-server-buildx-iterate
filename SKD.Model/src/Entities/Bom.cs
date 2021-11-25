namespace SKD.Model;

public class Bom : EntityBase {
    public Guid PlantId { get; set; }
    public Plant Plant { get; set; }
    public int Sequence { get; set; }
    public bool LotPartQuantitiesMatchShipment { get; set; }
    public ICollection<Lot> Lots { get; set; } = new List<Lot>();
}
