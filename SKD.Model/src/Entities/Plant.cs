namespace SKD.Model;

public class Plant : EntityBase {
    public string Code { get; set; }
    public string Name { get; set; }

    public string PartnerPlantCode { get; set; }
    public string PartnerPlantType { get; set; }

    public ICollection<Lot> Lots { get; set; } = new List<Lot>();
    public ICollection<KitSnapshotRun> KitSnapshotRuns { get; set; } = new List<KitSnapshotRun>();
    public ICollection<Bom> Boms { get; set; } = new List<Bom>();
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    public ICollection<KitVinImport> KitVinImports { get; set; } = new List<KitVinImport>();
}
