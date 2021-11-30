namespace SKD.Model;

public partial class Lot : EntityBase {
    public string LotNo { get; set; } = "";
    public string Note { get; set; } = "";

    public Guid PlantId { get; set; }
    public virtual Plant Plant { get; set; }

    public Guid BomId { get; set; }
    public virtual Bom Bom { get; set; }

    public Guid ModelId { get; set; }
    public virtual VehicleModel Model { get; set; }

    public ICollection<Kit> Kits { get; set; } = new List<Kit>();
    public ICollection<LotPart> LotParts { get; set; } = new List<LotPart>();
    public ICollection<ShipmentLot> ShipmentLots { get; set; } = new List<ShipmentLot>();
}
