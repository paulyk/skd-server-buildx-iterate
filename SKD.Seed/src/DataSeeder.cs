using System;
using System.IO;
using SKD.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace SKD.Seed {
   public class DataSeeder {
      
       public async Task GenerateSeedData(AppDbContext ctx) {
            var seedDataPath = Path.Combine(Directory.GetCurrentDirectory(),"src/json");

            var seedData = new SeedData(seedDataPath);
            var generator = new Generator(ctx);

            await generator.DroCreateDb();

            await generator.Seed_Components(seedData.Component_SeedData);
            await  generator.Seed_VehicleModels(seedData.VehicleModel_SeedData);
            await generator.Seed_VehicleModelComponents(seedData.VehicleModelComponent_SeedData); 
            await  generator.Seed_Vehicles(seedData.Vehicle_SeedData);
        }
    }
}
