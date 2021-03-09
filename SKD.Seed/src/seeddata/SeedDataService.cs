using System;
using System.IO;
using SKD.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace SKD.Seed {
    public class SeedDataService {

        SkdContext ctx;
        public SeedDataService(SkdContext ctx) {
            this.ctx = ctx;
        }

        public async Task GenerateReferencekData() {

            // drop & create
            var dbService = new DbService(ctx);
            await dbService.MigrateDb();

            if (await ctx.KitTimelineEventTypes.CountAsync() > 0) {
                // already seeded
                return;
            }

            // seed
            var seedDataPath = Path.Combine(Directory.GetCurrentDirectory(), "src/json");
            var seedData = new SeedData(seedDataPath);

            var generator = new SeedDataGenerator(ctx);
            await generator.Seed_VehicleTimelineVentType();
            await generator.Seed_Components(seedData.Component_MockData);
            await generator.Seed_ProductionStations(seedData.ProductionStation_MockData);
        }
    }
}
