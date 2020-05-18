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
using VT.Seed;

namespace VT.Server {
    public class Startup {

        public Startup(IConfiguration configuration, IWebHostEnvironment env) {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment _env { get; }

        public void ConfigureServices(IServiceCollection services) {
            
            services.AddDbContext<AppDbContext>(options => {            
                var databaseProviderName = Configuration["DatabaseProviderName"];

                var aspnet_env = _env.EnvironmentName != null 
                    ? _env.EnvironmentName : "Production";
                            
                var connectionString = Configuration.GetConnectionString(aspnet_env);

                if (connectionString == null) {
                    throw new Exception($"Connection string not found for ASPNETCORE_ENVIRONMENT: {aspnet_env} ");
                }

                switch (databaseProviderName) {
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

            app.Use(next => context =>  {
                Console.WriteLine("Hey " + context.Request.HttpContext.Request.Path);
                return next(context);
            });

            // find a more concise way of doing this.
            PlaygroundOptions opt = new PlaygroundOptions(

            );
            opt.Path = "/playground";
            opt.QueryPath = "/api";
            app.UsePlayground(opt);

            app.UseEndpoints(endpoints => {
                endpoints.MapGet("/test", async context => {
                    await context.Response.WriteAsync("Hello from test endpoint");
                });

                endpoints.MapGet("/reseed", async context => {
                    if (!_env.IsDevelopment()) {
                        throw new Exception("Database seeding in Development mode only");
                    }
                    var ctx = context.RequestServices.GetService<AppDbContext>();
                    if (ctx != null) {
                        var dataSeeder = new DataSeeder();
                        await dataSeeder.GenerateSeedData(ctx);
                    } else {
                        throw new Exception("RequestServices.GetService returned null AppDbContext ");
                    }
                });

            });

        }
    }
}
