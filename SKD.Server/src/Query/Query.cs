using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using HotChocolate;
using SKD.Model;
using HotChocolate.Types.Relay;
using HotChocolate.Types;
using SKD.Dcws;

namespace SKD.Server {

    public class Query {

        IConfiguration Configuration { get; }

        public Query(IConfiguration configuration) {
            Configuration = configuration;
        }

        public ConfigettingDTO GetServerConfigSettings() {
            var planBuldLead = 0;
            Int32.TryParse(Configuration[ConfigSettingKey.PlanBuildLeadTimeDays], out planBuldLead);

            return new ConfigettingDTO {
                DcwsServiceAddress = Configuration[ConfigSettingKey.DcwsServiceAddress],
                PlanBuildLeadTimeDays = planBuldLead
            };
        }
        public string Info() => "RMA SDK Server";

        [UsePaging]
        [UseSelection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Component> GetComponents([Service] SkdContext context) =>
             context.Components.AsQueryable();

        [UsePaging]
        [UseSelection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Part> GetParts([Service] SkdContext context) =>
             context.Parts.AsNoTracking().AsQueryable();

        [UsePaging]
        [UseSelection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Plant> GetPlants([Service] SkdContext context) =>
                 context.Plants.AsNoTracking().AsQueryable();

        [UsePaging]
        [UseSelection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Kit> GetVehicles(
            [Service] SkdContext context,
            string plantCode
        ) => context.Kits.Where(t => t.Lot.Plant.Code == plantCode).AsQueryable();

        [UsePaging]
        [UseSelection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Lot> GetVehicleLots(
            [Service] SkdContext context,
            string plantCode
        ) => context.Lots.Where(t => t.Plant.Code == plantCode).AsQueryable();

        [UsePaging]
        [UseSelection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<VehicleModel> GetVehicleModels([Service] SkdContext context) =>
                context.VehicleModels.AsQueryable();

        [UsePaging]
        [UseSelection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<VehicleModelComponent> GetVehicleModelComponents([Service] SkdContext context) =>
                context.VehicleModelComponents.AsQueryable();


        [UsePaging]
        [UseSelection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<KitComponent> GetVehicleComponents([Service] SkdContext context) =>
                context.KitComponents.AsQueryable();

        [UsePaging]
        [UseSelection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<ComponentSerial> GetComponentSerails([Service] SkdContext context) =>
                context.ComponentSerials
                        .Include(t => t.KitComponent)
                                .ThenInclude(t => t.Kit)
                                .ThenInclude(t => t.Lot)
                                .ThenInclude(t => t.Model)
                        .Include(t => t.KitComponent).ThenInclude(t => t.Component)
                        .Include(t => t.KitComponent).ThenInclude(t => t.ProductionStation)
                        .Include(t => t.DcwsResponses)
                        .AsQueryable();

        [UsePaging]
        [UseSelection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<DcwsResponse> GetDcwsResponses([Service] SkdContext context) =>
                context.DCWSResponses
                        .Include(t => t.ComponentSerial).ThenInclude(t => t.KitComponent)
                        .AsQueryable();

        [UsePaging]
        [UseSelection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<ProductionStation> GetProductionStations([Service] SkdContext context) =>
                context.ProductionStations.AsQueryable();

        [UsePaging]
        [UseSelection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Shipment> GetShipments(
            [Service] SkdContext context,
            string plantCode
        ) => context.Shipments.Where(t => t.Plant.Code == plantCode).AsQueryable();

        public async Task<ShipmentOverviewDTO?> GetShipmentOverview([Service] ShipmentService service, Guid shipmentId) =>
            await service.GetShipmentOverview(shipmentId);

        public async Task<Kit?> GetKitById([Service] SkdContext context, Guid id) {
            var result = await context.Kits.AsNoTracking()
                    .Include(t => t.Lot)
                    .Include(t => t.KitComponents).ThenInclude(t => t.Component)
                    .Include(t => t.KitComponents).ThenInclude(t => t.ProductionStation)
                    .Include(t => t.KitComponents).ThenInclude(t => t.ComponentSerials)
                    .Include(t => t.Lot).ThenInclude(t => t.Model)
                    .Include(t => t.TimelineEvents)
                    .FirstOrDefaultAsync(t => t.Id == id);

            return result;
        }

        public async Task<Kit?> GetKitByKitNo([Service] SkdContext context, string kitNo) {
            var result = await context.Kits.AsNoTracking()
                    .Include(t => t.Lot)
                    .Include(t => t.KitComponents).ThenInclude(t => t.Component)
                    .Include(t => t.KitComponents).ThenInclude(t => t.ProductionStation)
                    .Include(t => t.KitComponents).ThenInclude(t => t.ComponentSerials)
                    .Include(t => t.Lot).ThenInclude(t => t.Model)
                    .Include(t => t.TimelineEvents)
                    .FirstOrDefaultAsync(t => t.KitNo == kitNo);

            return result;
        }

        public async Task<VehicleTimelineDTO?> GetVehicleTimeline(
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

            var timelineEventTypes = await context.VehicleTimelineEventTypes.AsNoTracking()
                .OrderBy(t => t.Sequecne)
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
                            Sequence = evtType.Sequecne
                        }
                        : new TimelineEventDTO {
                            EventType = evtType.Code,
                            Sequence = evtType.Sequecne
                        };
                }).ToList()
            };

            return dto;
        }

        public async Task<Lot?> GetVehicleLotByLotNo([Service] SkdContext context, string lotNo) =>
                await context.Lots.AsNoTracking()
                        .Include(t => t.Model)
                        .Include(t => t.Kits)
                                .ThenInclude(t => t.TimelineEvents)
                                .ThenInclude(t => t.EventType)
                        .FirstOrDefaultAsync(t => t.LotNo == lotNo);

        public async Task<LotOverviewDTO?> GetLotOverview([Service] SkdContext context, string lotNo) {
            var lot = await context.Lots.OrderBy(t => t.LotNo).AsNoTracking()
                .Include(t => t.Kits).ThenInclude(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .Include(t => t.Model)
                .Include(t => t.Plant)
                .Include(t => t.Bom)
                .FirstOrDefaultAsync(t => t.LotNo == lotNo);

            if (lot == null) {
                return (LotOverviewDTO?)null;
            }

            var vehicle = lot.Kits.FirstOrDefault();
            var timelineEvents = lot.Kits.SelectMany(t => t.TimelineEvents);


            KitTimelineEvent? customReceivedEvent = null;
            if (vehicle != null) {
                customReceivedEvent = vehicle.TimelineEvents
                    .OrderByDescending(t => t.CreatedAt)
                    .Where(t => t.RemovedAt == null)
                    .FirstOrDefault(t => t.EventType.Code == TimeLineEventType.CUSTOM_RECEIVED.ToString());
            }

            return new LotOverviewDTO {
                Id = lot.Id,
                LotNo = lot.LotNo,
                BomId = lot.Bom.Id,
                BomSequenceNo = lot.Bom.Sequence,
                PlantCode = lot.Plant.Code,
                ModelCode = lot.Model.Code,
                ModelName = lot.Model.Name,
                CreatedAt = lot.CreatedAt,
                CustomReceived = customReceivedEvent != null 
                    ? new TimelineEventDTO {
                        EventType = TimeLineEventType.CUSTOM_RECEIVED.ToString(),
                        EventDate = customReceivedEvent != null ? customReceivedEvent.EventDate : (DateTime?)null,
                        EventNote = customReceivedEvent != null ? customReceivedEvent.EventNote : null,
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

        public async Task<List<Kit>> GetVehiclesByLot([Service] SkdContext context, string lotNo) =>
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

        public async Task<KitComponent?> GetVehicleComponentByVinAndComponent([Service] SkdContext context, string vin, string componentCode) =>
                 await context.KitComponents.AsNoTracking()
                        .Include(t => t.Kit)
                        .Include(t => t.Component)
                        .Include(t => t.ComponentSerials)
                        .FirstOrDefaultAsync(t => t.Kit.VIN == vin && t.Component.Code == componentCode);
        public async Task<ComponentSerial?> GetComponentScanById([Service] SkdContext context, Guid id) =>
                await context.ComponentSerials.AsNoTracking()
                        .Include(t => t.KitComponent).ThenInclude(t => t.Kit)
                        .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<ComponentSerial?> GetExistingComponentScan([Service] SkdContext context, Guid vehicleComponentId) =>
               await context.ComponentSerials.AsNoTracking()
                        .Include(t => t.KitComponent)
                        .FirstOrDefaultAsync(t => t.KitComponentId == vehicleComponentId && t.RemovedAt == null);

        [UsePaging]
        [UseSorting]
        public IQueryable<BomListDTO> GetBomList([Service] SkdContext context, string plantCode) =>
                context.Boms.AsNoTracking()
                    .Where(t => t.Plant.Code == plantCode)
                    .Select(t => new BomListDTO {
                        Id = t.Id,
                        PlantCode = t.Plant.Code,
                        Sequence = t.Sequence,
                        PartCount = t.Lots.SelectMany(t => t.LotParts).Select(t => t.Part).Distinct().Count(),
                        LotNumbers = t.Lots.Select(t => t.LotNo),
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
                        .Select(t => t.EventType.Code).FirstOrDefault(),
                        CreatedAt = t.CreatedAt
                    }).ToListAsync();

        public async Task<List<PartQuantityDTO>> GetBomPartsQuantity([Service] SkdContext context, Guid id) {
            var result = await context.LotParts
                .Where(t => t.Lot.Bom.Id == id)
                .GroupBy(t => new {
                    PartNo = t.Part.PartNo, PartDesc = t.Part.PartDesc
                })
                .Select(g => new PartQuantityDTO {
                    PartNo = g.Key.PartNo,
                    PartDesc = g.Key.PartDesc,
                    Quantity = g.Sum(u => u.BomQuantity)
                }).ToListAsync();

            return result;
        }

        public async Task<KitSnapshotRunDTO?> GetVehicleSnapshotRunByDate(
                  [Service] KitSnapshotService service,
                  string plantCode,
                  DateTime runDate
        ) => await service.GetSnapshotRunByDate(plantCode, runDate);

        public async Task<KitSnapshotRunDTO?> GetVehicleSnapshotRun(
                  [Service] KitSnapshotService service,
                  string plantCode,
                  int sequence
        ) => await service.GetSnapshotRunBySequence(plantCode, sequence);


        public async Task<List<SnapshotDTO>> GetRecentVehicleSnapshotRuns(
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

        public async Task<List<LotPartDTO>> GetRecentLotPartsReceived(
            [Service] LotPartService service,
            int count = 100
        ) => await service.GetRecentLotPartsReceived(count);

        public async Task<SerialCaptureVehicleDTO?> GetKitInfo_ForSerialCapture(
            [Service] ComponentSerialService service,
            string vin
        ) => await service.GetKitInfo_ForSerialCapture(vin);

        public async Task<bool> PingDcwsService(
            [Service] DcwsService service
        ) => await service.CanConnectToService();

        public async Task<string> GetDcwsServiceVersion(
            [Service] DcwsService service
        ) => await service.GetServiceVersion();

    }
}
