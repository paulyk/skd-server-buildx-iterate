using System;

namespace SKD.Model {
    public class VehicleStatusSnapshots : EntityBase {
        public DateTime RunDate { get; set; }
        public Guid VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public Guid PlantId { get; set; }
        public Plant Plant { get; set; }
        public PartnerStatus_ChangeStatus ChangeStatusCode { get; set; }
        public TimeLineEventType TimelineEventCode { get; set; }
        
        // the following fields are deliberately redundant
        public string VIN { get; set; }
        public string DealerCode { get; set; }
        public string EngineSerialNumber { get; set; }
        public DateTime? CustomReceived { get; set; }
        public DateTime? PlanBuild { get; set; }
        public DateTime? BuildCompleted { get; set; }
        public DateTime? GateRelease { get; set; }
        public DateTime? Wholesale { get; set; }
    }
}