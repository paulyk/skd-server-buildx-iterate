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
            var stationCodes = codes.Where(code => !ctx.ProductionStations.Any(t => t.Code == code)).ToList();

            var productionStations = stationCodes.ToList().Select((code, index) => new ProductionStation {
                Code = code,
                Name = $"{code} name",
                SortOrder = index + 1
            });


            ctx.ProductionStations.AddRange(productionStations);
            ctx.SaveChanges();
            return ctx.ProductionStations.ToList();
        }

        public List<Component> Gen_Components(SkdContext ctx, params string[] codes) {
            var componentCodes = codes.ToList().Where(code => !ctx.Components.Any(t => t.Code == code));

            var components = componentCodes.ToList().Select(code => new Component {
                Code = code,
                Name = $"{code} name"
            });

            ctx.Components.AddRange(components);
            ctx.SaveChanges();
            return ctx.Components.ToList();
        }


        public VehicleModel Gen_VehicleModel(SkdContext ctx,
            string modelCode,
            List<(string componentCode, string stationCode)> component_stations_maps
        ) {
            Gen_Components(ctx, component_stations_maps.Select(t => t.componentCode).ToArray());
            Gen_ProductionStations(ctx, component_stations_maps.Select(t => t.stationCode).ToArray());
            //
            component_stations_maps.ForEach(item => {
                var component = ctx.Components.FirstOrDefault(t => t.Code == item.componentCode);
            });

            var modelComponents = component_stations_maps
            .Select(map => new VehicleModelComponent {
                Component = ctx.Components.First(t => t.Code == map.componentCode),
                ProductionStation = ctx.ProductionStations.First(t => t.Code == map.stationCode)
            }).ToList();

            var vehicleModel = new VehicleModel {
                Code = modelCode,
                Name = $"{modelCode} name",
                ModelComponents = modelComponents
            };

            ctx.VehicleModels.Add(vehicleModel);
            ctx.SaveChanges();
            return vehicleModel;
        }

        public ComponentScan Gen_ComponentScan(SkdContext context, Guid vehicleComponentId) {
            var vehicleComponent = context.VehicleComponents.FirstOrDefault(t => t.Id == vehicleComponentId);
            var componentScan = new ComponentScan {
                VehicleComponentId = vehicleComponentId,
                Scan1 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry),
                Scan2 = ""
            };
            context.ComponentScans.Add(componentScan);
            context.SaveChanges();
            return componentScan;
        }

        public Vehicle Gen_VehicleModel_With_Vehicle(
            SkdContext ctx,
            string vin,
            string lotNo,
            string modelCode,
            List<(string componentCode, string stationCode)> component_stations_maps,
            DateTime? plannedBuildAt = null,
            DateTime? scanCompleteAt = null) {

            var vehicleModel = Gen_VehicleModel(ctx, modelCode, component_stations_maps);
            return Gen_Vehicle(ctx, vin, lotNo, modelCode, plannedBuildAt, scanCompleteAt);
        }
        public Vehicle Gen_Vehicle(SkdContext ctx,
            string vin,
            string lotNo,
            string modelCode,
            DateTime? plannedBuildAt = null,
            DateTime? scanCompleteAt = null
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
                ScanCompleteAt = scanCompleteAt,
                VehicleComponents = vehicleComponents
            };

            ctx.Vehicles.AddRange(vehicle);
            ctx.SaveChanges();

            return vehicle;
        }
    }
}