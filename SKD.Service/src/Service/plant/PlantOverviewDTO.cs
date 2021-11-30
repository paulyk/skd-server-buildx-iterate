#nullable enable

namespace SKD.Service;
public class PlantOverviewDTO {
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
