
using VT.Model;
using HotChocolate.Types;

namespace VT.Server {
    public class VehicleInput {
        public string Vin { get; set; } = "";
        public string ModelId { get; set; } = "";
        public string KitNo { get; set; } = "";
        public string LotNo { get; set; } = "";
    }
}