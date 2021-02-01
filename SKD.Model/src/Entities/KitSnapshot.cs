using System;

namespace SKD.Model {
    public class KitSnapshot : EntityBase {

        public Guid VehicleSnapshotRunId { get; set; }
        public kitSnapshotRun VehicleSnapshotRun { get; set; }
        public Guid KitId { get; set; }
        public Kit Kit { get; set; }
        public PartnerStatus_ChangeStatus ChangeStatusCode { get; set; }
        public TimeLineEventType TimelineEventCode { get; set; }
        public string VIN { get; set; }
        public string DealerCode { get; set; }
        public string EngineSerialNumber { get; set; }
        public DateTime? CustomReceived { get; set; }
        public DateTime? PlanBuild { get; set; }
        public DateTime? OrginalPlanBuild { get; set; }
        public DateTime? BuildCompleted { get; set; }
        public DateTime? GateRelease { get; set; }
        public DateTime? Wholesale { get; set; }
    }
}