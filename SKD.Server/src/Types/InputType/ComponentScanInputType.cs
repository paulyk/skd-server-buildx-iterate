using HotChocolate.Types;
using SKD.Model;

namespace SKD.Server {

    public class ComponentScanInputType : InputObjectType<ComponentScanDTO> {
        protected override void Configure(IInputObjectTypeDescriptor<ComponentScanDTO> descriptor) {
            base.Configure(descriptor);
            descriptor.Name("ComponentScanInput");
        }
    }
}

