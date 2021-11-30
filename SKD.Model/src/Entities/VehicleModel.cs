#nullable enable

namespace SKD.Model;

public partial class VehicleModel : EntityBase {
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelYear { get; set; } = "";
    public string Model { get; set; } = "";
    public string Series { get; set; } = "";
    public string Body { get; set; } = "";
    public ICollection<Lot> Lots { get; set; } = new List<Lot>();
    public ICollection<VehicleModelComponent> ModelComponents { get; set; } = new List<VehicleModelComponent>();
}
