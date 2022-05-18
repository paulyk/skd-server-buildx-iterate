namespace SKD.Model;

public class KitSnapshotRun : EntityBase {
    public Guid PlantId { get; set; }
    public Plant Plant { get; set; }
    public DateTime RunDate { get; set; }
    public int Sequence { get; set; }
    public ICollection<KitSnapshot> KitSnapshots { get; set; } = new List<KitSnapshot>();
    public PartnerStatusAck PartnerStatusAck { get; set; }
}