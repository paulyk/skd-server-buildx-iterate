namespace SKD.Model;

public class DcwsResponse : EntityBase {
    public string ProcessExcptionCode { get; set; } = "";
    public string ErrorMessage { get; set; }

    public Guid ComponentSerialId { get; set; }
    public ComponentSerial ComponentSerial { get; set; }

    public bool DcwsSuccessfulSave { get; set; }
}
