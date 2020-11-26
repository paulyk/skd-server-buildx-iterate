using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using HotChocolate;
using SKD.Model;
using HotChocolate.Types.Relay;
using HotChocolate.Types;

namespace SKD.Server {

    public class Query {

        public string Info() => "RMA vehicle component scanning service";

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
        public IQueryable<Vehicle> GetVehicles([Service] SkdContext context) =>
                 context.Vehicles.AsQueryable();

        [UsePaging]
        [UseSelection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<VehicleLot> GetVehicleLots([Service] SkdContext context) =>
                 context.VehicleLots.AsQueryable();

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
        public IQueryable<VehicleComponent> GetVehicleComponents([Service] SkdContext context) =>
                context.VehicleComponents.AsQueryable();

        [UsePaging]
        [UseSelection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<ComponentScan> GetComponentScans([Service] SkdContext context) =>
                context.ComponentScans
                        .Include(t => t.VehicleComponent)
                                .ThenInclude(t => t.Vehicle)
                                .ThenInclude(t => t.Model)
                        .Include(t => t.DCWSResponses)
                        .AsQueryable();

        [UsePaging]
        [UseSelection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<DCWSResponse> GetDcwsResponses([Service] SkdContext context) =>
                context.DCWSResponses
                        .Include(t => t.ComponentScan).ThenInclude(t => t.VehicleComponent)
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
        public IQueryable<Shipment> GetShipments([Service] SkdContext context) =>
                context.Shipments.AsQueryable();

        public async Task<Shipment?> GetShipmentDetailById([Service] SkdContext context, Guid id) =>
                await context.Shipments.AsNoTracking()
                        .Include(t => t.Lots).ThenInclude(t => t.Invoices).ThenInclude(t => t.Parts)
                        .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<Vehicle?> GetVehicleById([Service] SkdContext context, Guid id) {
            var result = await context.Vehicles.AsNoTracking()
                    .Include(t => t.Lot)
                    .Include(t => t.VehicleComponents).ThenInclude(t => t.Component)
                    .Include(t => t.VehicleComponents).ThenInclude(t => t.ProductionStation)
                    .Include(t => t.VehicleComponents).ThenInclude(t => t.ComponentScans)
                    .Include(t => t.Model)
                    .Include(t => t.TimelineEvents)
                    .FirstOrDefaultAsync(t => t.Id == id);

            return result;
        }
        public async Task<Vehicle?> GetVehicleByVinOrKitNo([Service] SkdContext context, string vinOrKitNo) {
            var result = await context.Vehicles.AsNoTracking()
                    .Include(t => t.Lot)
                    .Include(t => t.VehicleComponents).ThenInclude(t => t.Component)
                    .Include(t => t.VehicleComponents).ThenInclude(t => t.ProductionStation)
                    .Include(t => t.VehicleComponents).ThenInclude(t => t.ComponentScans)
                    .Include(t => t.Model)
                    .Include(t => t.TimelineEvents)
                    .FirstOrDefaultAsync(t => t.VIN == vinOrKitNo || t.KitNo == vinOrKitNo);

            return result;
        }

        public async Task<VehicleTimelineDTO?> GetVehicleTimeline([Service] SkdContext context, string vinOrKitNo) {
            var vehicle = await context.Vehicles.AsNoTracking()
                    .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                    .Include(t => t.Lot)
                    .FirstOrDefaultAsync(t => t.VIN == vinOrKitNo || t.KitNo == vinOrKitNo);

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

        public async Task<VehicleLot?> GetVehicleLotByLotNo([Service] SkdContext context, string lotNo) =>
                await context.VehicleLots.AsNoTracking()
                        .Include(t => t.Vehicles).ThenInclude(t => t.Model)
                        .Include(t => t.Vehicles)
                                .ThenInclude(t => t.TimelineEvents)
                                .ThenInclude(t => t.EventType)
                        .FirstOrDefaultAsync(t => t.LotNo == lotNo);

        public async Task<VehicleLotOverviewDTO?> GetVehicleLotOverview([Service] SkdContext context, string lotNo) {
            var vehicleLot = await context.VehicleLots.OrderBy(t => t.LotNo).AsNoTracking()
                .Include(t => t.Vehicles).ThenInclude(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .Include(t => t.Vehicles).ThenInclude(t => t.Model)
                .Include(t => t.Plant)
                .FirstOrDefaultAsync(t => t.LotNo == lotNo);

            if (vehicleLot == null) {
                return (VehicleLotOverviewDTO?)null;
            }

            var vehicle = vehicleLot.Vehicles.First();
            var timelineEvents = vehicleLot.Vehicles.SelectMany(t => t.TimelineEvents);

            var customReceivedEvent = vehicle.TimelineEvents
                .OrderByDescending(t => t.CreatedAt)
                .Where(t => t.RemovedAt == null)
                .FirstOrDefault(t => t.EventType.Code == TimeLineEventType.CUSTOM_RECEIVED.ToString());

            return new VehicleLotOverviewDTO {
                Id = vehicleLot.Id,
                LotNo = vehicleLot.LotNo,
                PlantCode = vehicleLot.Plant.Code,
                ModelCode = vehicle.Model.Code,
                ModelName = vehicle.Model.Name,
                CreatedAt = vehicleLot.CreatedAt,
                CustomReceived = new TimelineEventDTO {
                    EventType = TimeLineEventType.CUSTOM_RECEIVED.ToString(),
                    EventDate = customReceivedEvent != null ? customReceivedEvent.EventDate : (DateTime?)null,
                    EventNote = customReceivedEvent != null ? customReceivedEvent.EventNote : null,
                    CreatedAt = customReceivedEvent != null ? customReceivedEvent.CreatedAt : (DateTime?)null,
                    RemovedAt = customReceivedEvent != null ? customReceivedEvent.RemovedAt : (DateTime?)null
                }
            };
        }

        public async Task<List<Vehicle>> GetVehiclesByLot([Service] SkdContext context, string lotNo) =>
                 await context.Vehicles.OrderBy(t => t.Lot).AsNoTracking()
                    .Where(t => t.Lot.LotNo == lotNo)
                        .Include(t => t.Model)
                        .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                    .ToListAsync();

        public async Task<VehicleModel?> GetVehicleModelById([Service] SkdContext context, Guid id) =>
                await context.VehicleModels.AsNoTracking()
                        .Include(t => t.ModelComponents).ThenInclude(t => t.Component)
                        .Include(t => t.ModelComponents).ThenInclude(t => t.ProductionStation)
                    .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<Component?> GetComponentById([Service] SkdContext context, Guid id) =>
                 await context.Components.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);

        public async Task<VehicleComponent?> GetVehicleComponentByVinAndComponent([Service] SkdContext context, string vin, string componentCode) =>
                 await context.VehicleComponents.AsNoTracking()
                        .Include(t => t.Vehicle)
                        .Include(t => t.Component)
                        .Include(t => t.ComponentScans)
                        .FirstOrDefaultAsync(t => t.Vehicle.VIN == vin && t.Component.Code == componentCode);

        public async Task<VehicleOrComponentDTO> GetVehicleOrComponent([Service] SkdContext context, string vinOrCode) {
            Component component = await context.Components.AsNoTracking().FirstOrDefaultAsync(t => t.Code == vinOrCode);
            Vehicle? vehicle = null;

            if (component == null) {
                vehicle = await context.Vehicles
                        .Include(t => t.Model)
                        .Include(t => t.VehicleComponents).ThenInclude(t => t.Component)
                        .FirstOrDefaultAsync(t => t.VIN == vinOrCode);
            }

            return new VehicleOrComponentDTO {
                Code = vinOrCode,
                Vehicle = vehicle,
                Component = component
            };
        }

        public async Task<ComponentScan?> GetComponentScanById([Service] SkdContext context, Guid id) =>
                await context.ComponentScans.AsNoTracking()
                        .Include(t => t.VehicleComponent).ThenInclude(t => t.Vehicle)
                        .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<ComponentScan?> GetExistingComponentScan([Service] SkdContext context, Guid vehicleComponentId) =>
               await context.ComponentScans.AsNoTracking()
                        .Include(t => t.VehicleComponent)
                        .FirstOrDefaultAsync(t => t.VehicleComponentId == vehicleComponentId && t.RemovedAt == null);

        [UsePaging]
        [UseSorting]
        public IQueryable<BomSummaryListDTO> GetBomSummaryList([Service] SkdContext context) =>
                context.BomSummaries.AsNoTracking().Select(t => new BomSummaryListDTO {
                    Id = t.Id,
                    SequenceNo = t.SequenceNo,
                    CreatedAt = t.CreatedAt,
                    LotPartQuantitiesMatchShipment = t.LotPartQuantitiesMatchShipment,
                    PartsCount = t.Parts.Count(),
                }).AsQueryable();

        public async Task<BomSummary?> GetBomSummaryById([Service] SkdContext context, Guid id) =>
                await context.BomSummaries.AsNoTracking()
                        .Include(t => t.Parts)
                        .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<VehicleSnapshotsDTO> GetVehicleSnapshots(
                  [Service] VehicleSnapshotService service,
                  [Service] SkdContext ctx,
                  VehicleSnapshotInput input
        ) => await service.GetSnapshots(input);

        public async Task<List<DateTime>> GetVehiclSnapshotDates(
                  [Service] VehicleSnapshotService service,
                  [Service] SkdContext ctx,
                  string plantCode
        ) => await service.GetSnapshotDates(plantCode);                        

    }
}
