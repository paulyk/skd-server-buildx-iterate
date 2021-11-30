#nullable enable

namespace SKD.Service;

public class BomFile {
    public string PlantCode { get; set; } = "";
    public int Sequence { get; set; }
    public string BomFileCreatedAt { get; set; } = "";

    public List<BomFileLot> LotEntries { get; set; } = new List<BomFileLot>();
    public ICollection<BomFileLotPart> LotParts { get; set; } = new List<BomFileLotPart>();

    public class BomFileLot {
        public string LotNo { get; init; } = "";
        public List<BomFileKit> Kits { get; init; } = new List<BomFileKit>();

        public class BomFileKit {
            public string KitNo { get; init; } = "";
            public string ModelCode { get; init; } = ""; 
        }
    }

    public class BomFileLotPart {
        public string LotNo { get; init; } = "";
        public string PartNo { get; set; } = "";
        public string PartDesc { get; init; } = "";
        public int Quantity { get; set; }
    }
}
