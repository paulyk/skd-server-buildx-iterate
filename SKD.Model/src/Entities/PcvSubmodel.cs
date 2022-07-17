#nullable enable
namespace SKD.Model;

public class PcvSubmodel : EntityBase {
    public String Code { get; set; } = "";
    public String Name { get; set; } = "";

    public PcvModel Model { get; set; } = new PcvModel();
    public Guid ModelId { get; set; }

    public ICollection<PcvSubmodelComponent> SubmodelComponents { get; set; } = new List<PcvSubmodelComponent>();
    public ICollection<PCV> Pcvs { get; set; } = new List<PCV>();
}