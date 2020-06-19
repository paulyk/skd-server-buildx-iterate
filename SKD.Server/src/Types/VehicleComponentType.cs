# nullable enable

using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SKD.Model;
using System.Threading.Tasks;
using HotChocolate.Resolvers;

namespace SKD.Server {

    public class VehicleComponentType : ObjectType<VehicleComponent> {

        protected override void Configure(IObjectTypeDescriptor<VehicleComponent> descriptor) {
            descriptor.Field(t => t.Id).Type<NonNullType<IdType>>();
            descriptor.Field(t => t.Component).Resolver(GetVehicleComponent_Component);
            descriptor.Field(t => t.Vehicle).Resolver(GetVehicleComponent_Vehicle);            
        }

        public async Task<Component> GetVehicleComponent_Component(IResolverContext ctx) {
            Console.WriteLine("GetVehicleComponent_Component");
            var dbCtx = ctx.Service<AppDbContext>();

            var parent = ctx.Parent<VehicleComponent>();
            if (parent.Component != null) {
                Console.WriteLine("Found parent vehicle");
                return parent.Component;
            }

            return await dbCtx.Components.AsNoTracking().FirstOrDefaultAsync(t => t.Id == parent.ComponentId);
        }

        public async Task<Vehicle> GetVehicleComponent_Vehicle(IResolverContext ctx) {
            Console.WriteLine("GetVehicleComponent_Vehicle");
            var dbCtx = ctx.Service<AppDbContext>();

            var parent = ctx.Parent<VehicleComponent>();
            if (parent.Vehicle != null) {
                return parent.Vehicle;
            }

            return await dbCtx.Vehicles.AsNoTracking().FirstOrDefaultAsync(t => t.Id == parent.VehicleId);
        }
    }
}