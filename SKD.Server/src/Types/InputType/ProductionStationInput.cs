using SKD.Model;
using HotChocolate.Types;

namespace SKD.Server {
    public class ProductionStationInput {
        public string? Id { get; set; } = "";
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
    }
}