# nullable enable

using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Resolvers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using VT.Model;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace VT.Server {

    public class VehicleModelType : ObjectType<VehicleModel> {

        protected override void Configure(IObjectTypeDescriptor<VehicleModel> descriptor) {

            descriptor.Field(t => t.Id).Type<NonNullType<IdType>>();
            // descriptor.Field(t => t.ComponentMappings).Ignore();
            descriptor.Field(t => t.ActiveComponentMappings).Ignore();
            descriptor.Field(t => t.ComponentMappings).Resolver(GetVheicleModel_Components);
            descriptor.Field(t => t.Vehicles).Resolver(GetVehicleModel_Vehicles);
        }

        public async Task<List<VehicleModelComponent>> GetVheicleModel_Components(IResolverContext ctx) {
            Console.WriteLine("GetVheicleModel_Components");
            return await ctx.Service<AppDbContext>().VehicleModelComponents
                     .AsNoTracking()
                     .Include(t => t.Component)
                     .Where(t => t.VehicleModelId == ctx.Parent<VehicleModel>().Id)
                     .ToListAsync();
        }

        public async Task<List<Vehicle>> GetVehicleModel_Vehicles(IResolverContext ctx) {
            Console.WriteLine("GetVehicleModel_Vehicles");
            return await ctx.Service<AppDbContext>().Vehicles
                    .Where(t => t.ModelId == ctx.Parent<VehicleModel>().Id).ToListAsync();
        }
    }

}
