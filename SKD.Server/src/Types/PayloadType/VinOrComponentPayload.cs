using SKD.Model;

namespace SKD.Server {
    public class VehicleOrComponentDTO {
        public string Code { get; set; } = "";
        public Vehicle? Vehicle { get; set; }
        public Component? Component { get; set; }
    }
}