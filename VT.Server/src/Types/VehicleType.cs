# nullable enable

using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using VT.Model;
using System.Threading.Tasks;
using HotChocolate.Resolvers;

namespace VT.Server {

    public class VehicleType : ObjectType<Vehicle> {

        protected override void Configure(IObjectTypeDescriptor<Vehicle> descriptor) {

            descriptor.Field(t => t.Id).Type<NonNullType<IdType>>().Name("id");
            descriptor.Field(t => t.VIN).Type<NonNullType<StringType>>().Name("vin");
            descriptor.Field(t => t.ModelId).Ignore();
            descriptor.Field(t => t.VehicleComponents).Ignore();
            descriptor.Field(t => t.Model).Resolver(GetVehicleModel);
            descriptor.Field("componentCount").Type<NonNullType<IntType>>().Resolver(GetVehicleComponent_Count);
            descriptor.Field("components").Resolver(GetVehicleComponents);
        }

        public async Task<VehicleModel> GetVehicleModel(IResolverContext ctx) {
            Console.WriteLine("GetVehicleModel");
            return await ctx.Service<AppDbContext>().VehicleModels
                        .Where(t => t.Id == ctx.Parent<Vehicle>().ModelId)
                        .FirstOrDefaultAsync();
        }

        public async Task<List<VehicleComponent>> GetVehicleComponents(IResolverContext ctx) {
            Console.WriteLine("GetComponents");
            return await ctx.Service<AppDbContext>().VehicleComponents
                       .Where(t => t.VehicleId == ctx.Parent<Vehicle>().Id)
                       .Include(t => t.Component)
                       .ToListAsync();
        }

        public async Task<int> GetVehicleComponent_Count(IResolverContext ctx) {
            Console.WriteLine("GetVehicleComponent_Count");
             return await ctx.Service<AppDbContext>().VehicleComponents
                      .Where(t => t.VehicleId == ctx.Parent<Vehicle>().Id).CountAsync();
        }
    }
}