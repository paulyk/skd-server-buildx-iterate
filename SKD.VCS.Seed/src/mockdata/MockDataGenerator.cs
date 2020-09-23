using SKD.VCS.Model;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;

namespace SKD.VCS.Seed {
    internal class MockDataGenerator {
        private SkdContext ctx;

        public MockDataGenerator(SkdContext ctx) {
            this.ctx = ctx;
        }

        public async Task Seed_ProductionStations(ICollection<ProductionStation_Mock_DTO> data) {
            var stations = data.ToList().Select(x => new ProductionStation() {
                Code = x.code,
                Name = x.name,
                SortOrder = x.sortOrder,
                CreatedAt = Util.RandomDateTime(DateTime.UtcNow)
            });

            ctx.ProductionStations.AddRange(stations);
            await ctx.SaveChangesAsync();

            Console.WriteLine($"Added {ctx.ProductionStations.Count()} production stations");
        }

        public async Task Seed_Components(ICollection<Component_MockData_DTO> componentData) {
            var components = componentData.ToList().Select(x => new Component() {
                Code = x.code,
                Name = x.name,
                CreatedAt = Util.RandomDateTime(DateTime.UtcNow)
            });

            ctx.Components.AddRange(components);
            await ctx.SaveChangesAsync();

            Console.WriteLine($"Added {ctx.Components.Count()} components");
        }

    }
}