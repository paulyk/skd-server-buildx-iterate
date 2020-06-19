
using SKD.Model;
using HotChocolate.Types;

namespace SKD.Server {
    public class VehicleInput {
        public string Vin { get; set; } = "";
        public string ModelId { get; set; } = "";
        public string KitNo { get; set; } = "";
        public string LotNo { get; set; } = "";
    }
}