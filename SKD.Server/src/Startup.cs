
namespace SKD.Server;

public class Startup {

    public Startup(IConfiguration configuration, IWebHostEnvironment env) {
        Configuration = configuration;
        _env = env;
    }

    public IConfiguration Configuration { get; }
    public IWebHostEnvironment _env { get; }

    private int ExecutionTimeoutSeconds {
        get {
            Int32.TryParse(Configuration[ConfigSettingKey.ExecutionTimeoutSeconds], out int executionTimeoutSeconds);
            return executionTimeoutSeconds;
        }
    }

    public void ConfigureServices(IServiceCollection services) {        

        services.AddApplicationInsightsTelemetry();

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
                sqlOptions => {
                    sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                    sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
                }
            );
        });

        services
            .AddScoped<SearchService>()
            .AddScoped<KitService>(sp =>
                new KitService(sp.GetRequiredService<SkdContext>(), currentDate: DateTime.Now))
            .AddScoped<KitSnapshotService>()
            .AddScoped<VehicleModelService>()
            .AddScoped<ComponentService>()
            .AddScoped<DCWSResponseService>()
            .AddScoped<ProductionStationService>()
            .AddScoped<ComponentSerialService>(sp =>
                new ComponentSerialService(sp.GetRequiredService<SkdContext>()))
            .AddScoped<ShipmentService>()
            .AddScoped<BomService>()
            .AddScoped<PlantService>()
            .AddScoped<LotPartService>()
            .AddScoped<HandlingUnitService>()
            .AddScoped<QueryService>().AddSingleton<DcwsService>(sp => new DcwsService(Configuration[ConfigSettingKey.DcwsServiceAddress]))
            .AddScoped<PartnerStatusBuilder>()
            .AddScoped<KitVinAckBuilder>()
            .AddScoped<VerifySerialService>()
            .AddScoped<DevMutation>(sp => new DevMutation(_env.IsDevelopment()));

        services.AddGraphQLServer()
            .AddQueryType<Query>()
                .AddTypeExtension<ProjectionQueries>()
            .AddMutationType<Mutation>()
                 .AddTypeExtension<DevMutation>()
            .AddType<VehicleType>()
            .AddType<SerialCaptureVehicleDTOType>()
            .AddType<ComponentSerialDtoType>()
            .AddType<VinFileInputType>()
            .AddType<VinFileType>()
            .AddType<VehicleTimelineDTOType>()
            .AddType<VehicleModelType>()
            .AddType<VehicleComponentType>()
            .AddType<KitListItemDtoType>()
            .AddType<KitVinType>()
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .AddInMemorySubscriptions()
            .ModifyRequestOptions(opt => {
                opt.IncludeExceptionDetails = _env.IsDevelopment();
                if (ExecutionTimeoutSeconds > 0) {
                    opt.ExecutionTimeout = TimeSpan.FromSeconds(ExecutionTimeoutSeconds);
                }
            });

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
                    await service.GenerateReferenceData();
                    context.Response.StatusCode = 200;
                });
                ep.MapGet("/ping", async context => {
                    await context.Response.WriteAsync("Ping!");
                });
            }
        });

    }
}

