
namespace SKD.Server;

public class Startup {

    public Startup(IConfiguration configuration, IWebHostEnvironment env) {
        Configuration = configuration;
        _env = env;
    }

    public IConfiguration Configuration { get; }
    public IWebHostEnvironment _env { get; }

    public void ConfigureServices(IServiceCollection services) {     
        var appSettings = new AppSettings(Configuration);   

        services.AddApplicationInsightsTelemetry();

        services.AddCors(options => {
            options.AddDefaultPolicy(
                builder => {
                    builder.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
                }
            );
        });

        services.AddDbContext<SkdContext>(options => {
            var connectionString = appSettings.DefaultConnectionString;
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
            .AddScoped<QueryService>().AddSingleton<DcwsService>(sp => new DcwsService(appSettings.DcwsServiceAddress))
            .AddScoped<PartnerStatusBuilder>()
            .AddScoped<KitVinAckBuilder>()
            .AddScoped<VerifySerialService>()
            .AddScoped<DevMutation>(sp => new DevMutation(_env.IsDevelopment()));

        services.AddGraphQLServer()
            .AllowIntrospection(appSettings.AllowGraphqlIntrospection)
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
                if (appSettings.ExecutionTimeoutSeconds > 0) {
                    opt.ExecutionTimeout = TimeSpan.FromSeconds(appSettings.ExecutionTimeoutSeconds);
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

        app.UseEndpoints(builder => {

            builder.MapGraphQL();

            builder.MapGet("/", async (context) => {
                await context.Response.WriteAsync("");
            });

            if (_env.IsDevelopment()) {
                builder.MapPost("/gen_ref_data", async (context) => {
                    var ctx = context.RequestServices.GetService<SkdContext>();
                    var service = new SeedDataService(ctx);
                    await service.GenerateReferenceData();
                    context.Response.StatusCode = 200;
                });
                builder.MapGet("/ping", async context => {
                    await context.Response.WriteAsync("Ping!");
                });
            }
        });
    }
}

