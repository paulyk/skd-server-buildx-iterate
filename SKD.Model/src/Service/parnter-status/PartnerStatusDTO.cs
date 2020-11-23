using System;
using System.Collections.Generic;

namespace SKD.Model {

    public enum PartnerStatus_TxType {
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

    public class PartnerStatusDTO {

        public DateTime RunDate { get; set; }
        public string PlantCode { get; set; }
        public ICollection<VehicleStatus> VehicleStatusEntries { get; set; } = new List<VehicleStatus>();
        public class VehicleStatus {
            public PartnerStatus_TxType TxType { get; set; }
            public PartnerStatus_CurrentStatusType CurrentStatusType { get; set; }
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