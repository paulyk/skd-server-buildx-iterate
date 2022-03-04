

namespace SKD.Service;

public class KitTimelineEventInput {
    public string KitNo { get; init; }
    public TimeLineEventCode EventCode { get; init; }
    public DateTime EventDate { get; init; }
    public string EventNote { get; init; } = "";
    public string DealerCode { get; set; } = "";
}

