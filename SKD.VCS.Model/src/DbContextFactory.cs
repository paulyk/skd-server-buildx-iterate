using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SKD.VCS.Model {
    public class DbContextFactory : IDesignTimeDbContextFactory<SkdContext> {
        public SkdContext CreateDbContext(string[] args) {
          
            var Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();


            var databaseProviderName = Configuration["DatabaseProviderName"];
            var connectionString = Configuration.GetConnectionString("Development");

            if (databaseProviderName == null) {
                throw new Exception($"databaseProviderName not found");
            }
            if (connectionString == null) {
                throw new Exception($"Connection string not found for Development");
            }

            var optionsBuilder = new DbContextOptionsBuilder<SkdContext>();
            switch (databaseProviderName) {
                case "sqlite": optionsBuilder.UseSqlite(connectionString); break;
                case "sqlserver": optionsBuilder.UseSqlServer(connectionString); break;
                case "postgres": optionsBuilder.UseNpgsql(connectionString); break;
                default: throw new Exception($"supported providers are sqlite, sqlserver, postgres");
            }

            return new SkdContext(optionsBuilder.Options);
        }
    }
}