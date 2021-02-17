#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.Model {

    public class SearchService {

        private readonly SkdContext context;

        public SearchService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<IReadOnlyList<Kit>> SearchVehicles(string query) {
            query = query.Trim();
            if (query.Length == 0) {
                return new List<Kit>();
            }

            // try find exact match
            var exactMatch = await context.Kits.AsNoTracking().FirstOrDefaultAsync(t => t.VIN == query);
            if (exactMatch != null) {
                return new List<Kit>() { exactMatch };
            }

            // find where query matches part of vin
            var byVIN = await context.Kits.AsNoTracking().Where(t => t.VIN.Contains(query)).ToListAsync();

            // find where matches
            var byModel = await context.Kits
                .AsNoTracking()
                .Where(t => t.Lot.Model.Code.Contains(query) || t.Lot.Model.Name.Contains(query))
                .ToListAsync();

            return byVIN.Union(byModel).ToList();
        }

         public async Task<IReadOnlyList<Component>> SearchComponents(string query) {
            query = query.Trim();
            if (query.Length == 0) {
                return new List<Component>();
            }

            // try find exact code match
            var exactCode = await context.Components.AsNoTracking().FirstOrDefaultAsync(t => t.Code == query);
            if (exactCode != null) {
                return new List<Component>() { exactCode };
            }

            // try find exact name match
            var exactName = await context.Components.FirstOrDefaultAsync(t => t.Name == query);
            if (exactName != null) {
                return new List<Component>() { exactName };
            }

            // find where query matches part of code or name
            return await context.Components
                .AsNoTracking()
                .Where(t => t.Code.Contains(query) || t.Name.Contains(query)).ToListAsync();
        }

        public async Task<IReadOnlyList<VehicleModel>> SearchVehicleModels(string query) {
            query = query.Trim();
            if (query.Length == 0) {
                return new List<VehicleModel>();
            }

            // try find exact code match
            var exactCode = await context.VehicleModels.AsNoTracking().FirstOrDefaultAsync(t => t.Code == query);
            if (exactCode != null) {
                return new List<VehicleModel>() { exactCode };
            }

            // try find exact name match
            var exactName = await context.VehicleModels.AsNoTracking().FirstOrDefaultAsync(t => t.Name == query);
            if (exactName != null) {
                return new List<VehicleModel>() { exactName };
            }

            // find where query matches part of code or name
            return await context.VehicleModels
                .AsNoTracking()
                .Where(t => t.Code.Contains(query) || t.Name.Contains(query)).ToListAsync();
        }
    }
}