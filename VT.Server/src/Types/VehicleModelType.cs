# nullable enable

using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using VT.Model;

namespace VT.Server {

    public class VehicleModelType : ObjectType<VehicleModel> {

        protected override void Configure(IObjectTypeDescriptor<VehicleModel> descriptor) {

            descriptor.Field(t => t.Id).Type<NonNullType<IdType>>();
            descriptor.Field(t => t.ComponentMappings).Ignore();

            descriptor.Field("components")
                .Resolver(ctx => ctx.Service<AppDbContext>().VehicleModelComponents
                     .Where(t => t.VehicleModelId == ctx.Parent<VehicleModel>().Id)
                     .Select(t => new ComponentSequence {
                         Code = t.Component.Code,
                         Name = t.Component.Name,
                         Sequence = t.Sequence,
                         CreatedAt = t.CreatedAt,
                         RemovedAt = t.RemovedAt
                     })
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