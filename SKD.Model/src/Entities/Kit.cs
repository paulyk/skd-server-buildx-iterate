namespace SKD.Model;

public partial class Kit : EntityBase {
    public virtual string VIN { get; set; } = "";
    public string KitNo { get; set; } = "";

    public Guid LotId { get; set; }
    public virtual Lot Lot { get; set; }

    public Guid? DealerId { get; set; }
    public virtual Dealer Dealer { get; set; }

    public virtual ICollection<KitComponent> KitComponents { get; set; } = new List<KitComponent>();
    public virtual ICollection<KitTimelineEvent> TimelineEvents { get; set; } = new List<KitTimelineEvent>();
    public virtual ICollection<KitSnapshot> Snapshots { get; set; } = new List<KitSnapshot>();
    public virtual ICollection<KitVin> KitVins { get; set; }
}
