using SKD.Model;
using System;

namespace SKD.Service;
#nullable enable
public class LotTimelineEventInput {
    public string LotNo { get; init; } = "";
    public TimeLineEventCode EventCode { get; init; }
    public DateTime EventDate { get; init; }
    public string EventNote { get; init; }  = "";
}
