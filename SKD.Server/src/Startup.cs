using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SKD.Model;
using SKD.Dcws;
using SKD.Seed;
using GraphQL.Server.Ui.Voyager;
using HotChocolate.Data;

namespace SKD.Server {
    public class Startup {

        public Startup(IConfiguration configuration, IWebHostEnvironment env) {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment _env { get; }

        public void ConfigureServices(IServiceCollection services) {

            int planBuildLeadTimeDays = 6;
            Int32.TryParse(Configuration[ConfigSettingKey.PlanBuildLeadTimeDays], out planBuildLeadTimeDays);

            services.AddCors(options => {
                options.AddDefaultPolicy(
                    builder => {
                        builder.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
                    }
                );
            });

            services.AddDbContext<SkdContext>(options => {
                var connectionString = Configuration.GetConnectionString(ConfigSettingKey.DefaultConnectionString);
                options.UseSqlServer(
                    connectionString,
                    o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
                );
            });

            services
                .AddScoped<SearchService>()
                .AddScoped<KitService>(sp =>
                    new KitService(sp.GetRequiredService<SkdContext>(),currentDate: DateTime.Now,  planBuildLeadTimeDays))
                .AddScoped<KitSnapshotService>()
                .AddScoped<VehicleModelService>()
                .AddScoped<ComponentService>()
                .AddScoped<DCWSResponseService>()
                .AddScoped<ProductionStationService>()
                .AddScoped<ComponentSerialService>(sp => 
                    new ComponentSerialService(sp.GetRequiredService<SkdContext>(), new DcwsSerialFormatter()))
                .AddScoped<ShipmentService>()
                .AddScoped<BomService>()
                .AddScoped<PlantService>()
                .AddScoped<LotPartService>()
                .AddScoped<HandlingUnitService>()
                .AddScoped<QueryService>().AddSingleton<DcwsService>(sp => new DcwsService(Configuration[ConfigSettingKey.DcwsServiceAddress]));

            services.AddGraphQLServer()
                .AddQueryType<Query>()
                .AddMutationType<Mutation>()
                .AddType<VehicleType>()
                .AddType<SerialCaptureVehicleDTOType>()
                .AddType<ComponentSerialDtoType>()
                .AddType<AssignKitVinInputType>()
                .AddType<VehicleTimelineDTOType>()
                .AddType<VehicleModelType>()
                .AddType<VehicleComponentType>()
                .AddProjections()
                .AddFiltering()
                .AddSorting()
                .AddInMemorySubscriptions();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseWebSockets();

            app.UseRouting();

            app.UseCors();

            if (env.IsDevelopment()) {
                // app.Use(next => context => {
                //     var connectionString = Configuration.GetConnectionString("Default");
                //     Console.WriteLine(connectionString);
                //     Console.WriteLine("Request log: " + context.Request.HttpContext.Request.Path);
                //     return next(context);
                // });
            }

            app.UseEndpoints(ep => {

                ep.MapGraphQL();

                if (_env.IsDevelopment()) {
                    ep.MapPost("/gen_ref_data", async (context) => {
                        var ctx = context.RequestServices.GetService<SkdContext>();
                        var service = new SeedDataService(ctx);
                        await service.GenerateReferencekData();
                        context.Response.StatusCode = 200;
                    });
                    ep.MapGet("/ping", async context => {
                        await context.Response.WriteAsync("Ping!");
                    });
                }
            });

            app.UseGraphQLVoyager(new GraphQLVoyagerOptions {
                GraphQLEndPoint = "/graphql",
                Path = "/graphql-voyager"
            });

        }
    }
}
