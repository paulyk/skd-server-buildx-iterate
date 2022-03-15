namespace SKD.Service;

public class SnapshotDTO {
    public DateTime RunDate { get; set; }
    public int Sequence { get; set; }
    public string PlantCode { get; set; }
    public int SnapshotCount { get; set; }
    public int ChangedCount { get; set; }
    public DateTime? RemovedAt { get; set; }
}
