#nullable enable
namespace SKD.Service;

public class BomSummaryInput {
    public string PlantCode { get; set; } = "";
    public string PartnerCode { get; set; } = "";
    public int Sequence { get; init; }
    public ICollection<BomSummaryPartInput> Parts { get; set; } = new List<BomSummaryPartInput>();
}

public class BomSummaryPartInput {
    public string LotNo { get; init; } = "";
    public string PartNo { get; init; } = "";
    public string PartDesc { get; init; } = "";
    public int Quantity { get; init; }
}

