using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SKD.VCS.Model;
using System.Linq;
using System;

namespace SKD.VCS.Test {
    public class TestBase {

        public SkdContext GetAppDbContext() {

            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<SkdContext>()
                        .UseSqlite(connection)
                        .Options;

            var ctx = new SkdContext(options);

            ctx.Database.EnsureCreated();
            return ctx;
        }

        public List<ProductionStation> Gen_ProductionStations(SkdContext ctx, params string[] codes) {
            var productionStations = codes.ToList().Select((code,index) => new ProductionStation {
                Code = code,
                Name = $"{code} name",
                SortOrder = index + 1
            });


            ctx.ProductionStations.AddRange(productionStations);
            ctx.SaveChanges();
            return ctx.ProductionStations.ToList();
        }

        public List<Component> Gen_Components(SkdContext ctx, params string[] codes) {
            var components = codes.ToList().Select(code => new Component {
                Code = code,
                Name = $"{code} name"
            });
            ctx.Components.AddRange(components);
            ctx.SaveChanges();
            return ctx.Components.ToList();
        }


        public VehicleModel Gen_VehicleModel(SkdContext ctx,
            string code,
            List<(string componentCode, string stationCode)> component_stations_maps
        ) {

            var modelComponents = component_stations_maps
            .Select(map => new VehicleModelComponent {
                Component = ctx.Components.First(t => t.Code == map.componentCode),
                ProductionStation = ctx.ProductionStations.First(t => t.Code == map.stationCode)
            }).ToList();

            var vehicleModel = new VehicleModel {
                Code = code,
                Name = $"{code} name",
                ModelComponents = modelComponents
            };

            ctx.VehicleModels.Add(vehicleModel);
            ctx.SaveChanges();
            return vehicleModel;
        }

        public Vehicle Gen_Vehicle(SkdContext ctx,
            string vin,
            string lotNo,
            string modelCode,
            DateTime? plannedBuildAt = null,
            DateTime? scanLockAt = null
            ) {

            var vehicleModel = ctx.VehicleModels
                .Include(t => t.ModelComponents)
                .FirstOrDefault(t => t.Code == modelCode);

            var vehicleComponents = vehicleModel.ModelComponents.Select(mc => new VehicleComponent {
                ComponentId = mc.ComponentId,
                ProductionStationId = mc.ProductionStationId
            }).ToList();

            var vehicleLot = new VehicleLot { LotNo = lotNo };
            ctx.VehicleLots.Add(vehicleLot);

            var vehicle = new Vehicle {
                VIN = vin,
                Lot = vehicleLot,
                Model = vehicleModel,
                PlannedBuildAt = plannedBuildAt,
                ScanCompleteAt = scanLockAt,
                VehicleComponents = vehicleComponents
            };

            ctx.Vehicles.AddRange(vehicle);
            ctx.SaveChanges();

            return vehicle;
        }
    }
}