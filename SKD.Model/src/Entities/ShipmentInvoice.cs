#nullable enable

namespace SKD.Model;
public class ShipmentInvoice : EntityBase {
    public string InvoiceNo { get; set; } = "";
    public DateTime ShipDate { get; set; }

    public Guid ShipmentLotId { get; set; }
    public ShipmentLot ShipmentLot { get; set; } = new ShipmentLot();

    public ICollection<HandlingUnit> HandlingUnits { get; set; } = new List<HandlingUnit>();
}
