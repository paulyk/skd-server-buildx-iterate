

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace VT.Model {

    public class ComponentService {
        private readonly AppDbContext context;

        public ComponentService(AppDbContext ctx) {
            this.context = ctx;
        }
        public async Task<IReadOnlyList<Component>> SearchComponents(string query) {
            query = query.Trim();
            if (query.Length == 0) {
                return new List<Component>();
            }

            // try find exact code match
            var exactCode = await context.Components.FirstOrDefaultAsync(t => t.Code == query);
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
    }
}
