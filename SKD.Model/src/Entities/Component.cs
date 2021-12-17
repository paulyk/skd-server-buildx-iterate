namespace SKD.Model;

public enum ComponentSerialRule {
    ANY,
    SERIAL_1_ONLY,
    SERIAL_1_AND_2,
    VIN_BODY
}
public class Component : EntityBase {
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string IconURL { get; set; } = "";

    public ComponentSerialRule ComponentSerialRule { get; set; }

    public ICollection<VehicleModelComponent> VehicleModelComponents { get; set; }
    public ICollection<KitComponent> KitComponents { get; set; }

    public Component() : base() {
        VehicleModelComponents = new List<VehicleModelComponent>();
        KitComponents = new List<KitComponent>();
    }
}
