
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace SKD.Server {
    public class DbContextHelper {
       

        public DbContextOptionsBuilder GetDbContextOptions(IConfiguration config) {
            
            var provider = config["provider"];
            var connectionString = config["connectionString"];

            var optionsBuilder = new DbContextOptionsBuilder();
            
            if (provider.ToLower() == "sqlite") {
                optionsBuilder.UseSqlite(connectionString);
            } else if (provider.ToLower() == "sqlserver") {
                optionsBuilder.UseSqlServer(connectionString);
            } else {
                throw new Exception("appsettings provider sqlite, sqlserver not specified");
            }

            return optionsBuilder;
        }

    }
}