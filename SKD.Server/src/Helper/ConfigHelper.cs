
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace SKD.Server {
    public class ConfigHelper {
        public IConfiguration GetConfig() {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables();

            var config = builder.Build();
            return config;
        }   
    }
}