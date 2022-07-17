#nullable enable
namespace SKD.Model;

public class PcvSubmodelComponent : EntityBase {

    public PcvSubmodel Submodel { get; set; } = new PcvSubmodel();
    public Guid SubmodelId { get; set; }

    public Component Component { get; set; } = new Component();
    public Guid ComponentId { get; set; }

}
