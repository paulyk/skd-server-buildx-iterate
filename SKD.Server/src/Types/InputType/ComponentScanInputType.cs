using HotChocolate.Types;
using SKD.Model;

namespace SKD.Server {

    public class ComponentScanInputType : InputObjectType<ComponentSerialInput> {
        protected override void Configure(IInputObjectTypeDescriptor<ComponentSerialInput> descriptor) {
            base.Configure(descriptor);
            descriptor.Name("ComponentScanInput");
        }
    }
}

