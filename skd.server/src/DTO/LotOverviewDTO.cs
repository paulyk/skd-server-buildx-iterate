#nullable enable
using System;

namespace SKD.Server;
public class LotOverviewDTO {
    public Guid Id { get; set; }

    public Guid BomId { get; set; }
    public int BomSequence { get; set; }
    public Guid ShipmentId { get; set; }
    public int ShipmentSequence { get; set; }
    public string LotNo { get; set; } = "";
    public string Note { get; set; } = "";
    public string PlantCode { get; set; } = "";
    public string ModelCode { get; set; } = "";
    public string ModelName { get; set; } = "";
    public TimelineEventDTO? CustomReceived { get; set; }
    public DateTime CreatedAt { get; set; }
}
