using SKD.Model;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
namespace SKD.Server {
    public class QueryType: ObjectType<Query> {
        protected override void Configure(IObjectTypeDescriptor<Query> descriptor) {
              descriptor.Field("getComponents")
                .Resolver(ctx => ctx.Service<AppDbContext>().Components.AsQueryable())
                .UsePaging<ComponentType>();

            descriptor.Field("getVehicleModles")
                .Resolver(ctx => ctx.Service<AppDbContext>().VehicleModels.AsQueryable())
                .UsePaging<VehicleModelType>();

            descriptor.Field("getVehicles")
                .Resolver(ctx => ctx.Service<AppDbContext>().Vehicles.AsQueryable())
                .UsePaging<VehicleType>();
        }
    }
}
