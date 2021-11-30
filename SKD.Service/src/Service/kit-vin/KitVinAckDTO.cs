#nullable enable

namespace SKD.Service;
public class KitVinAckDTO {
    public string PlantCode { get; set; } = "";
    public int Sequence { get; set; }
    public string ErrorMessage { get; set; } = "";
    public string Filename { get; set; } = "";
    public string PayloadText { get; set; } = "";
}
