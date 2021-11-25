namespace SKD.Model;

public class ComponentSerial : EntityBase {

    public Guid KitComponentId { get; set; }
    public virtual KitComponent KitComponent { get; set; }

    public string Serial1 { get; set; } = "";
    public string Serial2 { get; set; } = "";

    public string Original_Serial1 { get; set; } = "";
    public string Original_Serial2 { get; set; } = "";

    public DateTime? VerifiedAt { get; set; }

    public ICollection<DcwsResponse> DcwsResponses { get; set; } = new List<DcwsResponse>();

    public ComponentSerial() : base() { }
}
