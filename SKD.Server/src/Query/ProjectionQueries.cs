namespace SKD.Server;

[ExtendObjectType(typeof(Query))]
public class ProjectionQueries {

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Component> GetComponents([Service] SkdContext context)
        => context.Components.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Part> GetParts(
        [Service] SkdContext context
    ) => context.Parts;

    [UseProjection]
    public IQueryable<Plant> GetPlants([Service] SkdContext context) =>
             context.Plants;


    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Kit> GetKits(
        [Service] SkdContext context,
        string plantCode
    ) => context.Kits
        .Where(t => t.Lot.Plant.Code == plantCode)
        .AsQueryable();


    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Lot> GetLots(
        [Service] SkdContext context,
        string plantCode
    ) => context.Lots.Where(t => t.Plant.Code == plantCode).AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<VehicleModel> GetVehicleModels(
        [Service] SkdContext context
    ) => context.VehicleModels;

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<VehicleModelComponent> GetVehicleModelComponents(
        [Service] SkdContext context
    ) => context.VehicleModelComponents;


    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<KitComponent> GetKitComponents(
        [Service] SkdContext context
    ) => context.KitComponents;

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ComponentSerial> GetComponentSerails(
        [Service] SkdContext context
    ) => context.ComponentSerials;


    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<DcwsResponse> GetDcwsResponses(
        [Service] SkdContext context
    ) => context.DCWSResponses;


    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ProductionStation> GetProductionStations([Service] SkdContext context) =>
            context.ProductionStations;


    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Shipment> GetShipments(
        [Service] SkdContext context
    ) => context.Shipments;

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ShipmentPart> GetShipmentParts(
        [Service] SkdContext context
    ) => context.ShipmentParts;


    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<HandlingUnit> GetHandlingUnits(
        [Service] SkdContext context
    ) => context.HandlingUnits;

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<KitVinImport> GetVinImports(
        [Service] SkdContext context
    ) => context.KitVinImports;

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<KitSnapshotRun> GetKitSnapshotRuns(
        [Service] SkdContext context
    ) => context.KitSnapshotRuns;

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<KitSnapshot> GetKitSnapshos(
        [Service] SkdContext context
    ) => context.KitSnapshots;

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Dealer> GetDealers([Service] SkdContext context)
        => context.Dealers.AsQueryable();

}
