using System;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Playground;
using HotChocolate.Execution.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SKD.Model;
using SKD.Seed;

namespace SKD.Server {
    public class Startup {

        public Startup(IConfiguration configuration, IWebHostEnvironment env) {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment _env { get; }

        public void ConfigureServices(IServiceCollection services) {

            services.AddCors(options => {
                options.AddDefaultPolicy(
                    builder => {
                        builder.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
                    }
                );
            });

            services.AddDbContext<SkdContext>(options => {
                var connectionString = Configuration.GetConnectionString("Default");
                options.UseSqlServer(connectionString,
                b => b.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery));
            }, ServiceLifetime.Transient);

            services
                .AddTransient<SearchService>()
                .AddTransient<VehicleService>()
                .AddTransient<VehicleSnapshotService>()
                .AddTransient<VehicleModelService>()
                .AddTransient<ComponentService>()
                .AddTransient<DCWSResponseService>()
                .AddTransient<ProductionStationService>()
                .AddTransient<ComponentSerialService>()
                .AddTransient<ShipmentService>()
                .AddTransient<BomService>()
                .AddTransient<PlantService>()
                .AddTransient<LotPartService>()
                .AddTransient<QueryService>();

            services.AddGraphQL(sp => SchemaBuilder.New()
                .AddServices(sp)
                .AddQueryType<Query>()
                .AddMutationType<Mutation>()
                .AddType<VehicleType>()
                .AddType<SerialCaptureVehicleDTOType>()
                .AddType<ComponentSerialDtoType>()
                .AddType<AssignKitVinInputType>()
                .AddType<VehicleTimelineDTOType>()
                .AddType<VehicleModelType>()
                .AddType<VehicleComponentType>()
                .Create(), new QueryExecutionOptions { ForceSerialExecution = true });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors();

            app.UseWebSockets().UseGraphQL("/gql");

            if (env.IsDevelopment()) {
                app.Use(next => context => {
                    var connectionString = Configuration.GetConnectionString("Default");
                    Console.WriteLine(connectionString);
                    Console.WriteLine("Request log: " + context.Request.HttpContext.Request.Path);
                    return next(context);
                });
            }

            var opt = new PlaygroundOptions() {
                Path = "/playground",
                QueryPath = "/gql"
            };
            app.UsePlayground(opt);

            app.UseEndpoints(endpoints => {

                if (_env.IsDevelopment()) {
                    endpoints.MapPost("/gen_ref_data", async (context) => {
                        var ctx = context.RequestServices.GetService<SkdContext>();
                        var service = new SeedDataService(ctx);
                        await service.GenerateReferencekData();
                        context.Response.StatusCode = 200;
                    });

                    endpoints.MapPost("/migrate_db", async (context) => {
                        var ctx = context.RequestServices.GetService<SkdContext>();
                        var dbService = new DbService(ctx);
                        await dbService.MigrateDb();
                        context.Response.StatusCode = 200;
                    });

                    endpoints.MapPost("/reset_db", async (context) => {
                        var ctx = context.RequestServices.GetService<SkdContext>();
                        var dbService = new DbService(ctx);
                        await dbService.DroCreateDb();
                        var service = new SeedDataService(ctx);
                        await service.GenerateReferencekData();
                        context.Response.StatusCode = 200;
                    });

                    endpoints.MapGet("/ping", async context => {
                        await context.Response.WriteAsync("Ping!");
                    });
                }
            });
        }
    }
}
