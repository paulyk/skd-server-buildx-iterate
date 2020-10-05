using HotChocolate.Types;
using SKD.VCS.Model;

namespace SKD.VCS.Server {
    public class ComponentScanType: ObjectType<ComponentScan> {
        protected override void Configure(IObjectTypeDescriptor<ComponentScan> descriptor) {
            descriptor.Field(t => t.DCWS_ResponseAt).Name("dcws_ResponseAt");
            descriptor.Field(t => t.DCWS_ResponseCode).Name("dcws_ResponseCode");
        }
    }
}
