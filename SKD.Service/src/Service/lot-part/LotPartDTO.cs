namespace SKD.Service;

public class LotPartDTO {
    public string LotNo { get; set; }
    public string PartNo { get; set; }
    public string PartDesc { get; set; }
    public int BomQuantity { get; set; }
    public int ShipmentQuantity { get; set; }
    public int ReceivedQuantity { get; set; }
    public DateTime ImportDate { get; set; }
    public DateTime? RemovedDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
}
