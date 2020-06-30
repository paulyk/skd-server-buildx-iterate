using SKD.Model;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using System.Linq;
using HotChocolate.Resolvers;

namespace SKD.Server {
    public class QueryType : ObjectType<Query> {
        protected override void Configure(IObjectTypeDescriptor<Query> descriptor) {


            /// <summary>
            /// Gets all components.
            /// </summary>
            descriptor.Field("Components")
              .Resolver(GetComponents)
              .UsePaging<ComponentType>()
              .UseFiltering()
              .UseSorting();

            descriptor.Field("VehicleModles")
                .Resolver(GetVehicleModles)
                .UsePaging<VehicleModelType>();

            descriptor.Field("Vehicles")
                .Resolver(GetVehicles)
                .UsePaging<VehicleType>();

        }

        public IQueryable<Component> GetComponents(IResolverContext ctx) {
            return ctx.Service<AppDbContext>().Components.AsQueryable();
        }
        public IQueryable<VehicleModel> GetVehicleModles(IResolverContext ctx) {
            return ctx.Service<AppDbContext>().VehicleModels.AsQueryable();
        }

        public IQueryable<Vehicle> GetVehicles(IResolverContext ctx) {
            return ctx.Service<AppDbContext>().Vehicles.AsQueryable();
        }

    }
}
