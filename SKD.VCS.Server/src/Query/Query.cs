using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using HotChocolate;
using SKD.VCS.Model;
using HotChocolate.Types.Relay;
using HotChocolate.Types;

namespace SKD.VCS.Server {

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
                await context.Shipments
                        .Include(t => t.Lots).ThenInclude(t => t.Invoices).ThenInclude(t => t.Parts)
                        .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<Vehicle?> GetVehicleById([Service] SkdContext context, Guid id) {
                var result = await context.Vehicles
                        .Include(t => t.Lot)
                        .Include(t => t.VehicleComponents).ThenInclude(t => t.Component)
                        .Include(t => t.VehicleComponents).ThenInclude(t => t.ProductionStation)
                        .Include(t => t.VehicleComponents).ThenInclude(t => t.ComponentScans)
                        .Include(t => t.Model)
                        .Include(t => t.Timeline)
                        .FirstOrDefaultAsync(t => t.Id == id);

                return result;
        }

        public async Task<Vehicle?> GetVehicleByVin([Service] SkdContext context, string vin) {
                var result = await context.Vehicles
                        .Include(t => t.Lot)
                        .Include(t => t.VehicleComponents).ThenInclude(t => t.Component)
                        .Include(t => t.VehicleComponents).ThenInclude(t => t.ProductionStation)
                        .Include(t => t.VehicleComponents).ThenInclude(t => t.ComponentScans)
                        .Include(t => t.Model)
                        .Include(t => t.Timeline)
                        .FirstOrDefaultAsync(t => t.VIN == vin);

                return result;
        }
        public async Task<Vehicle?> GetVehicleByVinOrKitNo([Service] SkdContext context, string vinOrKitNo) {
                var result = await context.Vehicles
                        .Include(t => t.Lot)                        
                        .Include(t => t.VehicleComponents).ThenInclude(t => t.Component)
                        .Include(t => t.VehicleComponents).ThenInclude(t => t.ProductionStation)
                        .Include(t => t.VehicleComponents).ThenInclude(t => t.ComponentScans)
                        .Include(t => t.Model)
                        .Include(t => t.Timeline)
                        .FirstOrDefaultAsync(t => t.VIN== vinOrKitNo || t.KitNo == vinOrKitNo);

                return result;
        }

        public async Task<VehicleLot?> GetVehicleLotByLotNo([Service] SkdContext context, string lotNo) =>
                await context.VehicleLots
                        .Include(t => t.Vehicles).ThenInclude(t => t.Model)
                        .FirstOrDefaultAsync(t => t.LotNo == lotNo);

        public async Task<VehicleModel?> GetVehicleModelById([Service] SkdContext context, Guid id) =>
                await context.VehicleModels
                        .Include(t => t.ModelComponents).ThenInclude(t => t.Component)
                        .Include(t => t.ModelComponents).ThenInclude(t => t.ProductionStation)
                        .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<Component?> GetComponentById([Service] SkdContext context, Guid id) =>
                 await context.Components.FirstOrDefaultAsync(t => t.Id == id);

        public async Task<VehicleComponent?> GetVehicleComponentByVinAndComponent([Service] SkdContext context, string vin, string componentCode) =>
                 await context.VehicleComponents
                        .Include(t => t.Vehicle)
                        .Include(t => t.Component)
                        .Include(t => t.ComponentScans)
                        .FirstOrDefaultAsync(t => t.Vehicle.VIN == vin && t.Component.Code == componentCode);

        public async Task<VehicleOrComponentDTO> GetVehicleOrComponent([Service] SkdContext context, string vinOrCode) {
            Component component = await context.Components.FirstOrDefaultAsync(t => t.Code == vinOrCode);
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
                await context.ComponentScans
                        .Include(t => t.VehicleComponent).ThenInclude(t => t.Vehicle)
                        .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<ComponentScan?> GetExistingComponentScan([Service] SkdContext context, Guid vehicleComponentId) =>
               await context.ComponentScans
                        .Include(t => t.VehicleComponent)
                        .FirstOrDefaultAsync(t => t.VehicleComponentId == vehicleComponentId && t.RemovedAt == null);

        [UsePaging]
        [UseSorting]
        public IQueryable<BomSummaryListDTO> GetBomSummaryList([Service] SkdContext context) =>
                context.BomSummaries.Select(t => new BomSummaryListDTO {
                    Id = t.Id,
                    SequenceNo = t.SequenceNo,
                    CreatedAt = t.CreatedAt,
                    LotPartQuantitiesMatchShipment = t.LotPartQuantitiesMatchShipment,
                    PartsCount = t.Parts.Count(),        
                }).AsQueryable();

        public async Task<BomSummary?> GetBomSummaryById([Service] SkdContext context, Guid id) =>
                await context.BomSummaries
                        .Include(t => t.Parts)
                        .FirstOrDefaultAsync(t => t.Id == id);

    }
}
