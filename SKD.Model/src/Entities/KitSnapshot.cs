namespace SKD.Model;

public enum SnapshotChangeStatus {
    Added,
    Changed,
    NoChange,
    Final
}

public enum FordTimeLineCode {
    FPCR,       // Custom Received               
    FPBP,       // Planed Build Date Set / Change
    FPBS,       // Build Start (Check if VIN should be changed)
    FPBC,       // Build Completed At     
    FPGR,       // Gate Release         
    FPWS        // Wholesale Date         
}
public class KitSnapshot : EntityBase {

    public Guid KitSnapshotRunId { get; set; }
    public KitSnapshotRun KitSnapshotRun { get; set; }
    public Guid KitId { get; set; }
    public Kit Kit { get; set; }
    public SnapshotChangeStatus ChangeStatusCode { get; set; }

    public Guid KitTimeLineEventTypeId { get; set; }
    public KitTimelineEventType KitTimeLineEventType { get; set; }

    public string VIN { get; set; }
    public string DealerCode { get; set; }
    public string EngineSerialNumber { get; set; }
    public DateTime? CustomReceived { get; set; }
    public DateTime? PlanBuild { get; set; }
    public DateTime? VINcheck { get; set; }
    public DateTime? OrginalPlanBuild { get; set; }
    public DateTime? BuildCompleted { get; set; }
    public DateTime? GateRelease { get; set; }
    public DateTime? Wholesale { get; set; }
}