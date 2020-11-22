using HotChocolate.Types;
using SKD.Model;

namespace SKD.Server {

    public class ComponentScanInputType : InputObjectType<ComponentScanInput> {
        protected override void Configure(IInputObjectTypeDescriptor<ComponentScanInput> descriptor) {
            base.Configure(descriptor);
            descriptor.Name("ComponentScanInput");
        }
    }
}

