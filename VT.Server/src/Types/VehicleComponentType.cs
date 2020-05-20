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

    public class VehicleComponentType : ObjectType<VehicleComponent> {

        protected override void Configure(IObjectTypeDescriptor<VehicleComponent> descriptor) {
            descriptor.Field(t => t.Id).Type<NonNullType<IdType>>();

            descriptor.Field(t => t.Component)
                .Resolver(ctx => {
                    var dbCtx = ctx.Service<AppDbContext>();

                    var parent = ctx.Parent<VehicleComponent>();
                    return parent.Component != null   
                        ? parent.Component
                        : dbCtx.Components.AsNoTracking().FirstOrDefault(t => t.Id == parent.ComponentId);
                });

            descriptor.Field(t => t.Vehicle)
                .Resolver(ctx => {
                    var dbCtx = ctx.Service<AppDbContext>();

                    var parent = ctx.Parent<VehicleComponent>();
                    return parent.Vehicle != null   
                        ? parent.Vehicle
                        : dbCtx.Vehicles.AsNoTracking().FirstOrDefault(t => t.Id == parent.VehicleId);
                });                
        }
    }
}