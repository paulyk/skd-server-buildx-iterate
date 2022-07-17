#nullable enable
namespace SKD.Model;

public class ProductionStation : EntityBase {
    public string Code { get; set; } = "";
    public string Name { get; set; }  = "";
    public int Sequence { get; set; }
    public ICollection<PcvComponent> ModelComponents { get; set; } = new List<PcvComponent>();
    public ICollection<KitComponent> KitComponents { get; set; } = new List<KitComponent>();
    public ICollection<Component> DefaultStationComponents { get; set; } = new List<Component>();
}
