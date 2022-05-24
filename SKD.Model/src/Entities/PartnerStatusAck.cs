#nullable enable
namespace SKD.Model {
    public class PartnerStatusAck : EntityBase {
        public int TotalProcessed { get; set; }
        public int TotalAccepted { get; set; }
        public int TotalRejected { get; set; }
        public DateTime FileDate { get; set; }

        public Guid KitSnapshotRunId { get; set; }
        public KitSnapshotRun KitSnapshotRun { get; set; } = new KitSnapshotRun();
    }
}