namespace SKD.Model;

public class ShipmentLot : EntityBase {

    public Guid ShipmentId { get; set; }
    public Shipment Shipment { get; set; }

    public Guid LotId { get; set; }
    public Lot Lot { get; set; }

    public ICollection<ShipmentInvoice> Invoices { get; set; } = new List<ShipmentInvoice>();
}