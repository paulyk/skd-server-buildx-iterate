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
    }
}