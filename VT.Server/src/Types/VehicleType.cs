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
            descriptor.Field(t => t.VehicleComponents).Ignore();

            descriptor.Field(t => t.Model)
                .Resolver(ctx => ctx.Service<AppDbContext>().VehicleModels
                        .Where(t => t.Id == ctx.Parent<Vehicle>().ModelId)
                        .FirstOrDefault());

            descriptor.Field("componentCount").Type<NonNullType<IntType>>()
              .Resolver(ctx => {
                  var count = ctx.Service<AppDbContext>().VehicleComponents
                      .Where(t => t.VehicleId == ctx.Parent<Vehicle>().Id).Count();
                  return count;
              });

            descriptor.Field("components")
                  .Resolver(ctx => {
                      Console.WriteLine($"select components {ctx.Parent<Vehicle>().VIN}");
                      var results1 = ctx.Service<AppDbContext>().VehicleComponents
                        .Where(t => t.VehicleId == ctx.Parent<Vehicle>().Id)
                        .Include(t => t.Component)
                        .ToList();

                      Console.WriteLine($"found  {results1.Count}");
                      var results = results1.Select(t => new VehicleComponent_DTO() {
                          Code = t.Component.Code,
                          Name = t.Component.Name,
                          SerialNumber = t.SerialNumber
                      }).ToList();

                      return results;
                  });

        }

        // public async Task<VehicleModel> ResolveVehicleModel([Parent] Vehicle vehicle, [Service] AppDbContext ctx) {
        //     return await ctx.VehicleModels.Where(t => t.Id == vehicle.ModelId).FirstOrDefaultAsync();
        // }
    }

    public class VehicleComponent_DTO {


        public string Code { get; set; } = "";

        public string Name { get; set; } = "";

        public string SerialNumber { get; set; } = "";

    }
}