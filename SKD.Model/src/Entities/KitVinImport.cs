namespace SKD.Model;

public class KitVinImport : EntityBase {
    public Guid PlantId { get; set; }
    public Plant Plant { get; set; }
    public string PartnerPlantCode { get; set; }
    public int Sequence { get; set; }

    public ICollection<KitVin> KitVins { get; set; } = new List<KitVin>();
}
