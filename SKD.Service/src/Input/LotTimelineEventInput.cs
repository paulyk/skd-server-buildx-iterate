using SKD.Model;
using System;

namespace SKD.Service;

public class LotTimelineEventInput {
    public string LotNo { get; init; }
    public TimeLineEventCode EventType { get; init; }
    public DateTime EventDate { get; init; }
    public string EventNote { get; init; }
}
