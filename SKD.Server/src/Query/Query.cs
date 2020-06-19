#nullable enable

using System;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SKD.Model;
using HotChocolate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SKD.Server {

    public class Query {

        public string Hello() => "Welcome to the RMA Vehicle Component Tracking Graphql Server";

        public async Task<IReadOnlyList<Component>> GetComponents([Service] AppDbContext ctx, int first = Int32.MaxValue) {
            return await ctx.Components
                .AsNoTracking().OrderBy(t => t.Code).Take(first).ToListAsync();
        }

        public async Task<Component> GetComponentByCode([Service] AppDbContext ctx, string code) {
            return await ctx.Components.AsNoTracking().FirstOrDefaultAsync(t => t.Code == code);
        }

        public async Task<IReadOnlyList<VehicleModel>> GetVehicleModels([Service] AppDbContext ctx, int first = Int32.MaxValue) {
            return await ctx.VehicleModels
                .AsNoTracking().OrderBy(t => t.Code).Take(first).ToListAsync();
        }

        public async Task<VehicleModel> GetVehicleModelByCode([Service] AppDbContext ctx, string code) {
            return await ctx.VehicleModels.AsNoTracking().FirstOrDefaultAsync(t => t.Code == code);
        }
        public async Task<IReadOnlyList<Vehicle>> GetVehicles([Service] AppDbContext ctx, int first = Int32.MaxValue) {
            return await ctx.Vehicles
                .AsNoTracking().OrderBy(t => t.Model.Code).Take(first).ToListAsync();
        }
        public async Task<Vehicle> GetVehicleByVIN([Service] AppDbContext ctx, string vin) {
            return await ctx.Vehicles.AsNoTracking().FirstOrDefaultAsync(t => t.VIN == vin);
        }

        public async Task<IReadOnlyList<Component>> SearcComponents([Service] SearchService service, string query) {
            return await service.SearchComponents(query);
        }

        public async Task<IReadOnlyList<Vehicle>> SearchVehicles([Service] SearchService service, string query) {
            return await service.SearchVehicles(query);
        }

        public async Task<IReadOnlyList<VehicleModel>> SearchVehicleModels([Service] SearchService service, string query) {
            return await service.SearchVehicleModels(query);
        }
    }
}