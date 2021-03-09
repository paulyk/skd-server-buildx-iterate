using System;

namespace SKD.Model {
    public class ShipmentOverviewDTO {
        public Guid Id { get; set; }
        public string PlantCode { get; set; }
        public int Sequence { get; set; }
        public int LotCount { get; set; }
        public int InvoiceCount { get; set; }
        public int HandlingUnitCount { get; set; }
        public int PartCount { get; set; }
        public DateTime CreatedAt {get; set; }
    }
}