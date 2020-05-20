# nullable enable

using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using VT.Model;
using Microsoft.EntityFrameworkCore;

namespace VT.Server {

    public class VehicleModelType : ObjectType<VehicleModel> {

        protected override void Configure(IObjectTypeDescriptor<VehicleModel> descriptor) {

            descriptor.Field(t => t.Id).Type<NonNullType<IdType>>();
            // descriptor.Field(t => t.ComponentMappings).Ignore();
            descriptor.Field(t => t.ActiveComponentMappings).Ignore();
       
            descriptor.Field(t => t.ComponentMappings)
                .Resolver(ctx => ctx.Service<AppDbContext>().VehicleModelComponents
                     .AsNoTracking()
                     .Include(t => t.Component)
                     .Where(t => t.VehicleModelId == ctx.Parent<VehicleModel>().Id)                     
                     .ToList()
                );                

            descriptor.Field(t => t.Vehicles)
                .Resolver(ctx => ctx.Service<AppDbContext>().Vehicles
                    .Where(t => t.ModelId == ctx.Parent<VehicleModel>().Id).ToList()
                );
        }
    }

    public class ComponentSequence {
        public Guid Id { get; set; }

        public string Code { get; set; } = "";

        public string Name { get; set; } = "";

        public int Sequence { get; set; } = 0;

        public DateTime CreatedAt { get; set; }
        public DateTime? RemovedAt { get; set; }
    }

}