using System;
using System.Collections.Generic;

namespace SKD.Model {

    public enum PartnerStatus_TxType {
        Added,
        Changed,
        NoChange,
        Final
    }

    public enum PartnerStatus_TimelineType {
        FPCR,       // Custom Received               
        FPBP,       // Planed Build Date Set / Change
        FPBC,       // Build Completed At     
        FPGR,       // Gate Release         
        FPWS        // Wholesale Date         
    }

    public class PartnerStatusDTO {

        public DateTime RunDate { get; set; }
        public string PlantCode { get; set; }
        public ICollection<ParnterStatusLine> Lines { get; set; } = new List<ParnterStatusLine>();
        public class ParnterStatusLine {
            public PartnerStatus_TxType TxType { get; set; }
            public string LotNo { get; set; }
            public string KitNo { get; set; }
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
}