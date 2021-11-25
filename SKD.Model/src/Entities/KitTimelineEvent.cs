namespace SKD.Model;
public class KitTimelineEvent : EntityBase {
    public Guid KitTimelineEventTypeId { get; set; }
    public KitTimelineEventType EventType { get; set; }
    public DateTime EventDate { get; set; }
    public string EventNote { get; set; }

    public Guid KitId { get; set; }
    public Kit Kit { get; set; }
}