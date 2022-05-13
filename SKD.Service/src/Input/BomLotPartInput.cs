namespace SKD.Service;
public class BomLotPartDTO {
    public string PlantCode { get; set; }
    public int Sequence { get; init; }
    public string BomFileCreatedAt { get; set; }

    public ICollection<BomLotPartItem> LotParts { get; set; } = new List<BomLotPartItem>();

    public class BomLotPartItem {
        public string LotNo { get; init; }
        public string PartNo { get; set; }
        public string PartDesc { get; init; }
        public int Quantity { get; set; }
    }
}
