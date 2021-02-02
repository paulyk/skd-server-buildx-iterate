using System;
using System.Collections.Generic;

namespace SKD.Model {

    public enum PartnerStatus_ChangeStatus {
        Added,
        Changed,
        NoChange,
        Final
    }

    public enum PartnerStatus_CurrentStatusType {
        FPCR,       // Custom Received               
        FPBP,       // Planed Build Date Set / Change
        FPBC,       // Build Completed At     
        FPGR,       // Gate Release         
        FPWS        // Wholesale Date         
    }

    public class KitSnapshotRunDTO {
        public DateTime RunDate { get; set; }
        public int Sequence { get; set; }
        public string PlantCode { get; set; }
        public ICollection<Entry> Entries { get; set; } = new List<Entry>();
        public class Entry {
            public PartnerStatus_ChangeStatus TxType { get; set; }
            public TimeLineEventType CurrentTimelineEvent { get; set; }
            public string LotNo { get; set; }
            public string KitNo { get; set; }
            public string VIN { get; set; }
            public string DealerCode { get; set; }
            public string EngineSerialNumber { get; set; }
            public DateTime? CustomReceived { get; set; }
            public DateTime? OriginalPlanBuild { get; set; }
            public DateTime? PlanBuild { get; set; }
            public DateTime? BuildCompleted { get; set; }
            public DateTime? GateRelease { get; set; }
            public DateTime? Wholesale { get; set; }
        }
    }
}