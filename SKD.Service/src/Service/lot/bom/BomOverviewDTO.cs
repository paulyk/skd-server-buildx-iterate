#nullable enable

namespace SKD.Model {
    public class BomOverviewDTO {
        public Guid Id { get; set; }
        public string PlantCode { get; set; } = "";
        public ICollection<BomShipInfoDTO> Shipments { get; set; } = new List<BomShipInfoDTO>();
        public int Sequence { get; set; }
        public int LotCount { get; set; }
        public int PartCount { get; set; }
        public int VehicleCount { get; set; }
        public DateTime CreatedAt {get; set; }
    }

    public class BomShipInfoDTO {
        public Guid ShipmentId { get; set;}
        public string PlantCode { get; set; } = "";
        public int Sequence { get; set; }
    }
}