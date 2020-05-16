using System;
using System.IO;
using VT.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace VT.Seed {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Seeding data");

            var config = GetConfig();
            var optionsBuilder = GetOptionsBuilder(config);
            var ctx = new AppDbContext(optionsBuilder.Options);
            GenerateSeedData(ctx);
        }

        static void GenerateSeedData(AppDbContext ctx) {
            var seedDataPath = Path.Combine(Directory.GetCurrentDirectory(),"src/json");

            var seedData = new SeedData(seedDataPath);
            var seeding = new Seeding(ctx);

            seeding.DroCreateDb();
            Console.WriteLine("recreated database");

            seeding.Seed_Components(seedData.Component_SeedData);
            seeding.Seed_VehicleModels(seedData.VehicleModel_SeedData);
            seeding.Seed_Vehicles(seedData.Vehicle_SeedData);
            seeding.Seed_VehicleModelComponents(seedData.VehicleModelComponent_SeedData); 
        }


        static DbContextOptionsBuilder GetOptionsBuilder(IConfiguration config) {
                var provider = config["provider"];
            var connectionString = config["connectionString"];

            var optionsBuilder = new DbContextOptionsBuilder();

            switch (provider) {
                case "sqlite": optionsBuilder.UseSqlite(connectionString); break;
                case "sqlserver": optionsBuilder.UseSqlServer(connectionString); break;
                case "postgres":optionsBuilder.UseNpgsql(connectionString); break;
                default: throw new Exception($"supported providers are sqlite, sqlserver, postgres");
            }     

            return optionsBuilder;                   
        }


        static IConfiguration GetConfig() {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables();
            
            var config = builder.Build();
            return config;
        }
    }
}
