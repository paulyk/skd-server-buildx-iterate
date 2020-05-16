using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace VT.Model {
    public class DbContextFactory : IDesignTimeDbContextFactory<AppDbContext> {
        public AppDbContext CreateDbContext(string[] args) {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionString = config["connectionString"];

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            var provider = config["provider"];
            if (provider.ToLower() == "sqlite") {
                optionsBuilder.UseSqlite(connectionString);
            } else if (provider.ToLower() == "sqlserver") {
                optionsBuilder.UseSqlServer(connectionString);
            } else {
                throw new Exception("appsettings provider sqlite, sqlserver not specified");
            }


            return new AppDbContext(optionsBuilder.Options);
        }
    }
}