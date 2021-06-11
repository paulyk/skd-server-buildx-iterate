using SKD.Model;
using System;

namespace SKD.Common {
    public class ComponentInput {
        public Guid? Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public DcwsSerialCaptureRule DcwsSerialCaptureRule { get; set; }
    }
}