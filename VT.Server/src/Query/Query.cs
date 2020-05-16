#nullable enable

using System;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using VT.Model;
using HotChocolate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VT.Server {

    public class Query {

        public string Hello() => "Welcome to the RMA Vehicle Component Tracking Graphql Server";

        public async Task<IReadOnlyList<Component>> GetComponents([Service] AppDbContext ctx, int first = Int32.MaxValue) {
            return await ctx.Components
                .AsNoTracking().OrderBy(t => t.Code).Take(first).ToListAsync();
        }
        public async Task<IReadOnlyList<VehicleModel>> GetVehicleModels([Service] AppDbContext ctx, int first = Int32.MaxValue) {
            return await ctx.VehicleModels
                .AsNoTracking().OrderBy(t => t.Code).Take(first).ToListAsync();
        }
        public IReadOnlyList<Vehicle> GetVehicles([Service] AppDbContext ctx, int first = Int32.MaxValue) {
            return ctx.Vehicles
                .AsNoTracking().OrderBy(t => t.Model.Code).ToList();
        }
        public async Task<IReadOnlyList<Component>> SearcComponents([Service] ComponentService service, string query) {
            return await service.SearchComponents(query);
        }

        public async Task<IReadOnlyList<Vehicle>> SearchVehicles([Service] VehicleService service, string query) {
            return await service.SearchVehicles(query);
        }

    }
}