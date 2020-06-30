using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using HotChocolate;
using SKD.Model;


namespace SKD.Server {

    public class Query {

        public string Info() => "RMA vehicle component scanning service";

        public async Task<Component> GetComponentByCode([Service] AppDbContext ctx, string code) {
            return await ctx.Components.AsNoTracking().FirstOrDefaultAsync(t => t.Code == code);
        }

        public async Task<VehicleModel> GetVehicleModelByCode([Service] AppDbContext ctx, string code) {
            return await ctx.VehicleModels.AsNoTracking().FirstOrDefaultAsync(t => t.Code == code);
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