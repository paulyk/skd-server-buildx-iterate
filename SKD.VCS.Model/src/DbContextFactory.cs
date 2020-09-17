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

            var connectionString = Configuration.GetConnectionString("Default");

            if (connectionString == null) {
                throw new Exception($"Default connection string not found for Development");
            }

            var optionsBuilder = new DbContextOptionsBuilder<SkdContext>();
            optionsBuilder.UseSqlServer(connectionString); 

            return new SkdContext(optionsBuilder.Options);
        }
    }
}