using HotChocolate.Types;
using SKD.VCS.Model;

namespace SKD.VCS.Server {

    public class ComponentScanInputType : InputObjectType<ComponentScanDTO> {
        protected override void Configure(IInputObjectTypeDescriptor<ComponentScanDTO> descriptor) {
            base.Configure(descriptor);
            descriptor.Name("ComponentScanInput");
        }
    }
}

