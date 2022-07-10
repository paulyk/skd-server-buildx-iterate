namespace SKD.Model;

public enum ComponentSerialRule {
    ONE_OR_BOTH_SERIALS,
    ONE_SERIAL,
    BOTH_SERIALS,
    VIN_AND_BODY
}
public class Component : EntityBase {
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string IconURL { get; set; } = "";

    public ComponentSerialRule ComponentSerialRule { get; set; }
    public bool DcwsRequired { get; set; }

    public ICollection<PcvComponent> VehicleModelComponents { get; set; }
    public ICollection<KitComponent> KitComponents { get; set; }

    public Component() : base() {
        VehicleModelComponents = new List<PcvComponent>();
        KitComponents = new List<KitComponent>();
    }
}
