#nullable enable

namespace SKD.Service;

public class SerialCaptureVehicleDTO {
    public string VIN { get; set; } = "";
    public string LotNo { get; set; } = "";
    public string ModelCode { get; set; } = "";
    public string ModelName { get; set; } = "";
    public List<SerialCaptureComponentDTO> Components { get; set; } = new List<SerialCaptureComponentDTO>();
}

public class SerialCaptureComponentDTO {
    public Guid KitComponentId { get; set; }
    public int ProductionStationSequence { get; set; }
    public string ProductionStationCode { get; set; } = "";
    public string ProductionStationName { get; set; } = "";
    public string ComponentCode { get; set; } = "";
    public string ComponentName { get; set; } = "";
    public string? Serial1 { get; set; } = "";
    public string? Serial2 { get; set; } = "";
    public DateTime? SerialCapturedAt { get; set; }
}
