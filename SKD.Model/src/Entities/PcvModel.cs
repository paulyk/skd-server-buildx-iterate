#nullable enable
namespace SKD.Model;

public class PcvModel : EntityBase {
    public String Code { get; set; } = "";
    public String Name { get; set; } = "";

    public ICollection<PcvSubmodel> Submodels { get; set; } = new List<PcvSubmodel>();
}