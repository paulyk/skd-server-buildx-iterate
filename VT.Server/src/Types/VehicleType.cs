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

namespace VT.Server {

    public class VehicleType : ObjectType<Vehicle> {

        protected override void Configure(IObjectTypeDescriptor<Vehicle> descriptor) {

            descriptor.Field(t => t.Id).Type<NonNullType<IdType>>().Name("id");

            descriptor.Field(t => t.VIN).Type<NonNullType<StringType>>().Name("vin");

            descriptor.Field(t => t.ModelId).Ignore();

            descriptor.Field(t => t.Model)
                .Resolver(ctx =>  ctx.Service<AppDbContext>().VehicleModels
                        .Where(t => t.Id == ctx.Parent<Vehicle>().ModelId)
                        .FirstOrDefault());

            descriptor.Field(t => t.VehicleComponents)
                .Resolver(ctx => ctx.Service<AppDbContext>().VehicleComponents.ToListAsync());

        }

        // public async Task<VehicleModel> ResolveVehicleModel([Parent] Vehicle vehicle, [Service] AppDbContext ctx) {
        //     return await ctx.VehicleModels.Where(t => t.Id == vehicle.ModelId).FirstOrDefaultAsync();
        // }
    }
}