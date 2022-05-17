namespace SKD.Service;
public class PartnerStatusAckFile {
    public string PlantCode { get; set; }
    public string PartnerPlantCode { get; set; }
    public int Sequence { get; set; }
    public string FileDate { get; set; }
    public string Status { get; set; }
    public int Total { get; set; }
    public int Accepted { get; set; }
    public int Rejected { get; set; }
}
