#nullable enable

namespace SKD.Model;

public class LotDTO {
    public string LotNo { get; set; } = "";

    public string Model { get; set; } = "";
    public string ModelCode { get; set; } = "";
    public string ModelDesc { get; set; } = "";
    public string ModelSeries { get; set; } = "";
    public string ModelBody { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}
