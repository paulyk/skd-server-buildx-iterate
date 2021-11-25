namespace SKD.Model;

public class HandlingUnit : EntityBase {
    public string Code { get; set; }
    public Guid ShipmentInvoiceId { get; set; }
    public ShipmentInvoice ShipmentInvoice { get; set; }
    public ICollection<ShipmentPart> Parts { get; set; } = new List<ShipmentPart>();
    public ICollection<HandlingUnitReceived> Received { get; set; } = new List<HandlingUnitReceived>();
}