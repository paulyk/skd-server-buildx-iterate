#nullable enable
namespace SKD.Service;

public class PartnerStatusAckDTO {
    public string PlantCode { get; set; } = "";
    public string PartnerPlantCode { get; set; } = "";
    public int Sequence { get; set; }
    public string FileDate { get; set; }  = "";
    public bool Accepted { get; set; }
    public int TotalProcessed { get; set; }
    public int TotalAccepted { get; set; }
    public int TotalRejected { get; set; }
}
