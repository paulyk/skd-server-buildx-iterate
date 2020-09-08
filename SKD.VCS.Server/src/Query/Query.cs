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
        public IQueryable<ComponentScan> GetVehicleComponentScans([Service] SkdContext context) =>
                context.ComponentScans.AsQueryable();


        [UsePaging]
        [UseSelection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<ProductionStation> GetProductionStations([Service] SkdContext context) =>
                context.ProductionStations.AsQueryable();

        
        public  async Task<Vehicle> GetVehicleByVIN([Service] SkdContext context, string vin) =>
                 await context.Vehicles
                        .Include(t => t.VehicleComponents).ThenInclude(t => t.Component)
                        .Include(t => t.VehicleComponents).ThenInclude(t => t.ProductionStation)
                        .Include(t => t.VehicleComponents).ThenInclude(t => t.ComponentScans)
                        .Include(t => t.Model)
                        .FirstOrDefaultAsync(t => t.VIN == vin);

        public  async Task<Component> GetComponentById([Service] SkdContext context, Guid id) =>
                 await context.Components.FirstOrDefaultAsync(t => t.Id == id);

    }
}