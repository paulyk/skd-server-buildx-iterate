using System;
using System.Collections.Generic;

namespace SKD.VCS.Model {

    public enum TimeLineEventType {
        CUSTOM_RECEIVED,
        PLAN_BUILD,
        BULD_COMPLETED,
        GATE_RELEASED,
        WHOLE_SALE
    }
    public partial class VehicleTimelineEventType : EntityBase {
        public string Code { get; set; } = "";
        public string Description { get; set; } = "";
        public int Sequecne { get; set; }
    }
}