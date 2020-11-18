using SKD.VCS.Model;

namespace SKD.VCS.Server {
    public class VehicleOrComponentDTO {
        public string Code { get; set; } = "";
        public Vehicle? Vehicle { get; set; }
        public Component? Component { get; set; }
    }
}