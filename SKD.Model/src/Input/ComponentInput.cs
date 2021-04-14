using System;

namespace SKD.Model {
    public class ComponentInput {
        public Guid? Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public SerialCaptureRequirement SerialCaptureRequirement { get; set; }
    }
}