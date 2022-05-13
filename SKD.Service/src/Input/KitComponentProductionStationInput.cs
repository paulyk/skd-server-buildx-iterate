#nullable enable
namespace SKD.Service;

public class KitComponentProductionStationInput {
    public Guid KitComponentId { get; init; }
    public string ProductionStationCode { get; init; } = "";
}
