using System;
using System.IO;
using SKD.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace SKD.Seed {
    public class MockDataService {

        SkdContext ctx;
        public MockDataService(SkdContext ctx) {
            this.ctx = ctx;
        }

        public async Task GenerateMockData() {

            // drop & create
            var dbService = new DbService(ctx);
            await dbService.DroCreateDb();

            // seed
            var seedDataPath = Path.Combine(Directory.GetCurrentDirectory(), "src/json");
            var seedData = new MockData(seedDataPath);

            var generator = new MockDataGenerator(ctx);
            await generator.Seed_Components(seedData.Component_SeedData);
            await generator.Seed_VehicleModels(seedData.VehicleModel_SeedData);
            await generator.Seed_VehicleModelComponents(seedData.VehicleModelComponent_SeedData);
            await generator.Seed_Vehicles(seedData.Vehicle_SeedData);
        }
    }
}
