using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Playground;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VT.Model;

namespace VT.Server {
    public class Startup {

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services) {
            Console.WriteLine("configure services");
            services.AddDbContext<AppDbContext>(options => {

                var providerName = Configuration["provider"];
                var connectionString = Configuration["connectionString"];

                switch (providerName) {
                    case "sqlite": options.UseSqlite(connectionString); break;
                    case "sqlserver": options.UseSqlServer(connectionString); break;
                    case "postgres":options.UseNpgsql(connectionString); break;
                    default: throw new Exception($"supported providers are sqlite, sqlserver, postgres");
                }                           
            });

            services
                .AddTransient<VehicleService>()
                .AddTransient<ComponentService>();

            services.AddGraphQL(sp => SchemaBuilder.New()
                .AddQueryType<QueryType>()
                .AddType<VehicleType>()
                .AddType<VehicleModelType>()
                .AddMutationType<Mutation>()
                .AddServices(sp)
                .Create());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
          
            app.UseRouting();

            app.UseGraphQL("/api");
            PlaygroundOptions opt = new PlaygroundOptions();
            opt.Path = "/playground";
            opt.QueryPath = "/api";
            app.UsePlayground(opt);


            app.UseEndpoints(endpoints => {
                endpoints.MapGet("/hello", async context => {
                    await context.Response.WriteAsync("Hello from endpoints");
                });
            });

        }
    }
}
