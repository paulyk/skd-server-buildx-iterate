#nullable enable
namespace SKD.Model;

public class ProductionStation : EntityBase {
    public string Code { get; set; } = "";
    public string Name { get; set; }  = "";
    public int Sequence { get; set; }
    public ICollection<VehicleModelComponent> ModelComponents { get; set; } = new List<VehicleModelComponent>();
    public ICollection<KitComponent> VehicleComponents { get; set; } = new List<KitComponent>();
}
