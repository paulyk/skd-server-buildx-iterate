#nullable enable

namespace SKD.Service;

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
            .Where(t => t.Lot.Pcv.Code.Contains(query) || t.Lot.Pcv.Description.Contains(query))
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

    public async Task<IReadOnlyList<PCV>> SearchVehicleModels(string query) {
        query = query.Trim();
        if (query.Length == 0) {
            return new List<PCV>();
        }

        // try find exact code match
        var exactCode = await context.Pcvs.AsNoTracking().FirstOrDefaultAsync(t => t.Code == query);
        if (exactCode != null) {
            return new List<PCV>() { exactCode };
        }

        // try find exact name match
        var exactName = await context.Pcvs.AsNoTracking().FirstOrDefaultAsync(t => t.Description == query);
        if (exactName != null) {
            return new List<PCV>() { exactName };
        }

        // find where query matches part of code or name
        return await context.Pcvs
            .AsNoTracking()
            .Where(t => t.Code.Contains(query) || t.Description.Contains(query)).ToListAsync();
    }
}
