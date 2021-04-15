using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SKD.Server {
    public class Program {
        public static void Main(string[] args) {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => {
                    logging.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder
                        .UseStartup<Startup>()
                        .ConfigureAppConfiguration(config => {
                            config.AddJsonFile("appsettings.json", optional: false)
                            .AddJsonFile("developer.json", optional: true)
                            .AddEnvironmentVariables();
                        });
                });
        }
    }
}
