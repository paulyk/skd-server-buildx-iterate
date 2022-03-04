#nullable enable
namespace SKD.Model;

public partial class Kit : EntityBase {
    public virtual string VIN { get; set; } = "";
    public string KitNo { get; set; } = "";

    public Guid LotId { get; set; }
    public Lot Lot { get; set; } = new Lot();

    public Guid? DealerId { get; set; }
    public Dealer? Dealer { get; set; } 

    public virtual ICollection<KitComponent> KitComponents { get; set; } = new List<KitComponent>();
    public virtual ICollection<KitTimelineEvent> TimelineEvents { get; set; } = new List<KitTimelineEvent>();
    public virtual ICollection<KitSnapshot> Snapshots { get; set; } = new List<KitSnapshot>();
    public virtual ICollection<KitVin> KitVins { get; set; } = new List<KitVin>();
}
