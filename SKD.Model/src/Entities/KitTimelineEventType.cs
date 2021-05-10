using System;
using System.Collections.Generic;

namespace SKD.Model {

    public enum TimeLineEventType {
        CUSTOM_RECEIVED,
        PLAN_BUILD,
        BUILD_COMPLETED,
        GATE_RELEASED,
        WHOLE_SALE
    }
    public partial class KitTimelineEventType : EntityBase {
        public string Code { get; set; } = "";
        public string Description { get; set; } = "";
        public int Sequecne { get; set; }
    }
}