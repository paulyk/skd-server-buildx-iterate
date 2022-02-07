#pragma warning disable
namespace SKD.Server;

public class Query {

    IConfiguration Configuration { get; }

    public Query(IConfiguration configuration) {
        Configuration = configuration;
    }

    public ConfigettingDTO GetServerConfigSettings() {
        if (Int32.TryParse(Configuration[ConfigSettingKey.PlanBuildLeadTimeDays], out int planBuildLeadTimeDays)) {

            return new ConfigettingDTO {
                DcwsServiceAddress = Configuration[ConfigSettingKey.DcwsServiceAddress],
                PlanBuildLeadTimeDays = planBuildLeadTimeDays
            };
        }
        return new ConfigettingDTO {
            DcwsServiceAddress = Configuration[ConfigSettingKey.DcwsServiceAddress],
            PlanBuildLeadTimeDays = 0
        };
    }

    public string Info() => "RMA SDK Server";

    public async Task<ShipmentOverviewDTO?> GetShipmentOverview(
        [Service] ShipmentService service,
        Guid shipmentId
    ) => await service.GetShipmentOverview(shipmentId);

    public async Task<List<HandlingUnitOverview>> GetHandlingUnitOverviews(
        [Service] HandlingUnitService service,
        Guid shipmentId
    ) => await service.GetHandlingUnitOverviews(shipmentId);

    public async Task<HandlingUnitInfoPayload?> GetHandlingUnitInfo(
        [Service] HandlingUnitService service,
        string huCode
    ) => await service.GetHandlingUnitInfo(huCode);

    public async Task<Kit?> GetKitById([Service] SkdContext context, Guid id) {
        var result = await context.Kits.AsNoTracking()
                .Include(t => t.Lot)
                .Include(t => t.Lot).ThenInclude(t => t.Model)
                .Include(t => t.KitComponents).ThenInclude(t => t.Component)
                .Include(t => t.KitComponents).ThenInclude(t => t.ProductionStation)
                .Include(t => t.KitComponents).ThenInclude(t => t.ComponentSerials)
                .Include(t => t.TimelineEvents)
                .Include(t => t.KitVins)
                .FirstOrDefaultAsync(t => t.Id == id);

        return result;
    }

    public async Task<Kit?> GetKitByKitNo([Service] SkdContext context, string kitNo) {
        var result = await context.Kits.AsNoTracking()
                .Include(t => t.Dealer)
                .Include(t => t.Lot).ThenInclude(t => t.Model).ThenInclude(t => t.ModelComponents).ThenInclude(t => t.Component)
                .Include(t => t.KitComponents).ThenInclude(t => t.Component)
                .Include(t => t.KitComponents).ThenInclude(t => t.ProductionStation)
                .Include(t => t.KitComponents)
                    .ThenInclude(t => t.ComponentSerials)
                    .ThenInclude(t => t.DcwsResponses)
                .Include(t => t.Lot).ThenInclude(t => t.Model)
                .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .Include(t => t.KitVins)
                .FirstOrDefaultAsync(t => t.KitNo == kitNo);

        return result;
    }
    public async Task<VehicleTimelineDTO?> GetKitTimeline(
        [Service] SkdContext context,
        string kitNo
    ) {
        var vehicle = await context.Kits.AsNoTracking()
                .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .Include(t => t.Lot)
                .FirstOrDefaultAsync(t => t.KitNo == kitNo);

        if (vehicle == null) {
            return (VehicleTimelineDTO?)null;
        }

        var timelineEventTypes = await context.KitTimelineEventTypes.AsNoTracking()
            .OrderBy(t => t.Sequence)
            .Where(t => t.RemovedAt == null).ToListAsync();

        var dto = new VehicleTimelineDTO {
            VIN = vehicle.VIN,
            KitNo = vehicle.KitNo,
            LotNo = vehicle.Lot.LotNo,
            TimelineItems = timelineEventTypes.Select(evtType => {
                var timelineEvent = vehicle.TimelineEvents
                    .Where(vt => vt.EventType.Code == evtType.Code)
                    .Where(vt => vt.RemovedAt == null)
                    .FirstOrDefault();

                return timelineEvent != null
                    ? new TimelineEventDTO {
                        EventDate = timelineEvent.EventDate,
                        EventNote = timelineEvent.EventNote,
                        EventType = timelineEvent.EventType.Code,
                        CreatedAt = timelineEvent.CreatedAt,
                        Sequence = evtType.Sequence
                    }
                    : new TimelineEventDTO {
                        EventType = evtType.Code,
                        Sequence = evtType.Sequence
                    };
            }).ToList()
        };

        return dto;
    }

    public async Task<Lot?> GetLotByLotNo([Service] SkdContext context, string lotNo) =>
            await context.Lots.AsNoTracking()
                    .Include(t => t.Model)
                    .Include(t => t.Kits)
                            .ThenInclude(t => t.TimelineEvents)
                            .ThenInclude(t => t.EventType)
                    .FirstOrDefaultAsync(t => t.LotNo == lotNo);

    public async Task<LotOverviewDTO?> GetLotOverview([Service] SkdContext context, string lotNo) {
        var lot = await context.Lots.OrderBy(t => t.LotNo).AsNoTracking()
            .Include(t => t.Kits)
                .ThenInclude(t => t.TimelineEvents)
                .ThenInclude(t => t.EventType)
            .Include(t => t.Model)
            .Include(t => t.Plant)
            .Include(t => t.Bom)
            .Include(t => t.ShipmentLots)
                .ThenInclude(t => t.Shipment)
            .FirstOrDefaultAsync(t => t.LotNo == lotNo);

        if (lot == null) {
            return (LotOverviewDTO?)null;
        }

        var kit = lot.Kits.FirstOrDefault();
        var timelineEvents = lot.Kits.SelectMany(t => t.TimelineEvents);

        KitTimelineEvent? customReceivedEvent = null;
        if (kit != null) {
            customReceivedEvent = kit.TimelineEvents
                .OrderByDescending(t => t.CreatedAt)
                .Where(t => t.RemovedAt == null)
                .FirstOrDefault(t => t.EventType.Code == TimeLineEventCode.CUSTOM_RECEIVED);
        }

        return new LotOverviewDTO {
            Id = lot.Id,
            LotNo = lot.LotNo,
            Note = lot.Note,
            BomId = lot.Bom.Id,
            BomSequence = lot.Bom.Sequence,
            ShipmentId = lot.ShipmentLots.Select(x => x.Shipment.Id).FirstOrDefault(),
            ShipmentSequence = lot.ShipmentLots.Select(x => x.Shipment.Sequence).FirstOrDefault(),
            PlantCode = lot.Plant.Code,
            ModelCode = lot.Model.Code,
            ModelName = lot.Model.Description,
            CreatedAt = lot.CreatedAt,
            CustomReceived = customReceivedEvent != null
                ? new TimelineEventDTO {
                    EventType = TimeLineEventCode.CUSTOM_RECEIVED,
                    EventDate = customReceivedEvent != null ? customReceivedEvent.EventDate : (DateTime?)null,
                    EventNote = customReceivedEvent?.EventNote,
                    CreatedAt = customReceivedEvent != null ? customReceivedEvent.CreatedAt : (DateTime?)null,
                    RemovedAt = customReceivedEvent != null ? customReceivedEvent.RemovedAt : (DateTime?)null
                }
                : null
        };
    }

    public async Task<List<LotPartDTO>> GetLotPartsByBom(
        [Service] QueryService service, Guid bomId) {
        return await service.GetLotPartsByBom(bomId);
    }

    public async Task<List<LotPartDTO>> GetLotPartsByShipment([Service] QueryService service, Guid shipmentId) =>
        await service.GetLotPartsByShipment(shipmentId);

    public async Task<List<Kit>> GetKitsByLot([Service] SkdContext context, string lotNo) =>
             await context.Kits.OrderBy(t => t.Lot).AsNoTracking()
                .Where(t => t.Lot.LotNo == lotNo)
                    .Include(t => t.Lot).ThenInclude(t => t.Model)
                    .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .ToListAsync();

    public async Task<VehicleModel?> GetVehicleModelById([Service] SkdContext context, Guid id) =>
            await context.VehicleModels.AsNoTracking()
                    .Include(t => t.ModelComponents).ThenInclude(t => t.Component)
                    .Include(t => t.ModelComponents).ThenInclude(t => t.ProductionStation)
                .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<Component?> GetComponentById([Service] SkdContext context, Guid id) =>
             await context.Components.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);

    [Obsolete("no longer used", error: true)]
    public async Task<KitComponent?> GetVehicleComponentByVinAndComponent([Service] SkdContext context, string vin, string componentCode) =>
             await context.KitComponents.AsNoTracking()
                    .Include(t => t.Kit)
                    .Include(t => t.Component)
                    .Include(t => t.ComponentSerials)
                    .FirstOrDefaultAsync(t => t.Kit.VIN == vin && t.Component.Code == componentCode);


    [Obsolete("no longer used", error: true)]
    public async Task<ComponentSerial?> GetComponentScanById([Service] SkdContext context, Guid id) =>
            await context.ComponentSerials.AsNoTracking()
                    .Include(t => t.KitComponent).ThenInclude(t => t.Kit)
                    .FirstOrDefaultAsync(t => t.Id == id);

    [Obsolete("no longer used", error: true)]
    public async Task<ComponentSerial?> GetExistingComponentScan([Service] SkdContext context, Guid vehicleComponentId) =>
           await context.ComponentSerials.AsNoTracking()
                    .Include(t => t.KitComponent)
                    .FirstOrDefaultAsync(t => t.KitComponentId == vehicleComponentId && t.RemovedAt == null);


    [UsePaging(MaxPageSize = 10000)]
    [UseSorting]
    public IQueryable<KitListItemDTO> GetKitList(
        [Service] SkdContext context,
        string plantCode
    ) =>
        context.Kits.AsNoTracking()
            .Where(t => t.Lot.Plant.Code == plantCode)
            .Select(t => new KitListItemDTO {
                Id = t.Id,
                LotNo = t.Lot.LotNo,
                KitNo = t.KitNo,
                VIN = t.VIN,
                ModelCode = t.Lot.Model.Code,
                ModelName = t.Lot.Model.Description,
                LastTimelineEvent = t.TimelineEvents
                    .OrderByDescending(t => t.CreatedAt)
                    .Select(t => t.EventType.Description)
                    .FirstOrDefault(),
                LastTimelineEventDate = t.TimelineEvents
                    .OrderByDescending(t => t.CreatedAt)
                    .Select(t => t.EventDate)
                    .FirstOrDefault(),
                ComponentCount = t.KitComponents
                    .Where(t => t.RemovedAt == null)
                    .Count(),
                ScannedComponentCount = t.KitComponents
                    .Where(t => t.RemovedAt == null)
                    .Where(t => t.ComponentSerials.Any(t => t.RemovedAt == null))
                    .Count(),
                VerifiedComponentCount = t.KitComponents
                    .Where(t => t.RemovedAt == null)
                    .Where(t => t.ComponentSerials.Any(u => u.RemovedAt == null && u.VerifiedAt != null))
                    .Count(),
                Imported = t.CreatedAt
            }).AsQueryable();


    [UsePaging(MaxPageSize = 10000)]
    [UseSorting]
    public IQueryable<BomListDTO> GetBomList([Service] SkdContext context, string plantCode) =>
            context.Boms.AsNoTracking()
                .Where(t => t.Plant.Code == plantCode)
                .Select(t => new BomListDTO {
                    Id = t.Id,
                    PlantCode = t.Plant.Code,
                    Sequence = t.Sequence,
                    PartCount = t.Lots.SelectMany(t => t.LotParts).Select(t => t.Part).Distinct().Count(),
                    Lots = t.Lots.Select(t => new BomListDTO.BomList_Lot {
                        LotNo = t.LotNo,
                        ShipmentSequence = t.ShipmentLots.Select(s => s.Shipment.Sequence).Any()
                            ? t.ShipmentLots.Select(s => s.Shipment.Sequence).First()
                            : null
                    }),
                    CreatedAt = t.CreatedAt
                }).AsQueryable();

    public async Task<Bom?> GetBomById([Service] SkdContext context, Guid id) =>
            await context.Boms.AsNoTracking()
                    .Include(t => t.Lots).ThenInclude(t => t.LotParts)
                    .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<BomOverviewDTO?> GetBomOverview([Service] BomService service, Guid id) =>
         await service.GetBomOverview(id);

    public async Task<List<LotListDTO>> GetLotListByBomId([Service] SkdContext context, Guid id) =>
             await context.Lots.AsNoTracking()
                .Where(t => t.Bom.Id == id)
                .Select(t => new LotListDTO {
                    Id = t.Id,
                    PlantCode = t.Plant.Code,
                    LotNo = t.LotNo,
                    KitCount = t.Kits.Count(),
                    TimelineStatus = t.Kits
                        .SelectMany(t => t.TimelineEvents)
                        .OrderByDescending(t => t.CreatedAt)
                        .Where(t => t.RemovedAt == null)
                        .Select(t => (TimeLineEventCode?)t.EventType.Code).FirstOrDefault(),
                    CreatedAt = t.CreatedAt
                }).ToListAsync();

    public async Task<List<PartQuantityDTO>> GetBomPartsQuantity([Service] SkdContext context, Guid id) {
        var result = await context.LotParts
            .Where(t => t.Lot.Bom.Id == id)
            .GroupBy(t => new {
                PartNo = t.Part.PartNo,
                PartDesc = t.Part.PartDesc
            })
            .Select(g => new PartQuantityDTO {
                PartNo = g.Key.PartNo,
                PartDesc = g.Key.PartDesc,
                Quantity = g.Sum(u => u.BomQuantity)
            }).ToListAsync();

        return result;
    }

    public async Task<KitSnapshotRunDTO?> GetKitSnapshotRunByDate(
              [Service] KitSnapshotService service,
              string plantCode,
              DateTime runDate
    ) => await service.GetSnapshotRunByDate(plantCode, runDate);

    public async Task<KitSnapshotRunDTO?> GetKitSnapshotRun(
              [Service] KitSnapshotService service,
              string plantCode,
              int sequence
    ) => await service.GetSnapshotRunBySequence(plantCode, sequence);


    public async Task<List<SnapshotDTO>> GetRecentKitSnapshotRuns(
              [Service] KitSnapshotService service,
              string plantCode,
              int count
    ) => await service.GetSnapshotRuns(plantCode, count);

    public async Task<LotDTO?> GetLotInfo(
           [Service] LotPartService service,
           string lotNo
    ) => await service.GetLotInfo(lotNo);

    public async Task<LotPartDTO?> GetLotPartInfo(
           [Service] LotPartService service,
           string lotNo,
           string partNo
    ) => await service.GetLotPartInfo(lotNo, partNo);

    public async Task<List<LotParReceivedtDTO>> GetRecentLotPartsReceived(
        [Service] LotPartService service,
        int count = 100
    ) => await service.GetRecentLotPartsReceived(count);

    public async Task<BasicKitInfo?> GetBasicKitInfo(
        [Service] ComponentSerialService service,
        string vin
    ) => await service.GetBasicKitInfo(vin);

    public async Task<KitComponentSerialInfo?> GetKitComponentSerialInfo(
        [Service] ComponentSerialService service,
        string kitNo,
        string componentCode
    ) => await service.GetKitComponentSerialInfo(kitNo, componentCode);

    public async Task<bool> PingDcwsService(
        [Service] DcwsService service
    ) => await service.CanConnectToService();

    public async Task<string> GetDcwsServiceVersion(
        [Service] DcwsService service
    ) => await service.GetServiceVersion();

    public async Task<PartnerStatusDTO> GetPartnerStatusFilePayload(
        [Service] SkdContext context,
        [Service] PartnerStatusBuilder service,
        string plantCode,
        int sequence
    ) => await service.GeneratePartnerStatusFilePaylaod(
            plantCode: plantCode,
            sequence: sequence
        );

    public BomFile ParseBomFile(string text) =>
        new BomFileParser().ParseBomFile(text);

    public ShipFile ParseShipFile(string text) =>
        new ShipFileParser().ParseShipmentFile(text);

    public VinFile ParseVinFile(string text) =>
        new VinFileParser().ParseVinFile(text);

    public Task<KitVinAckDTO> GenerateKitVinAcknowledgment([Service] KitVinAckBuilder kitVinAckBuilder, string plantCode, int sequence) =>
        kitVinAckBuilder.GenerateKitVinAcknowledgment(plantCode, sequence);

    public FordInterfaceFileType GetFordInterfaceFileType(string filename) =>
        FordInterfaceFileTypeService.GetFordInterfaceFileType(filename);

    public async Task<string> GenPartnerStatusFilename(
        [Service] PartnerStatusBuilder service,
        Guid kitSnapshotRunId
    ) => await service.GenPartnerStatusFilename(kitSnapshotRunId);

}

