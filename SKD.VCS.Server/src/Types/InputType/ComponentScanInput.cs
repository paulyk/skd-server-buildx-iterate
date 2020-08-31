using SKD.VCS.Model;
using HotChocolate.Types;
using System;

namespace SKD.VCS.Server {
    public class ComponentScanInput {
        public Guid vehicleComponentId { get; set; } = new Guid();
        public string Scan1 { get; set; } = "";
        public string Scan2 { get; set; } = "";
    }
}