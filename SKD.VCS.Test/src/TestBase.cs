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

        public void Gen_ProductionStations(SkdContext ctx, int count = 2) {
            var productionStations = (new int[count]).ToList().Select((x, i) => new ProductionStation {
                Code = $"Station_{i}",
                Name = $"Station_name_{i}"
            });

            ctx.ProductionStations.AddRange(productionStations);
            ctx.SaveChanges();
        }

        public void Gen_Components(SkdContext ctx, int count = 2) {
            var components = (new int[count]).ToList().Select((x, i) => new Component {
                Code = $"Component_{i}",
                Name = $"Component_{i}"
            });
            ctx.Components.AddRange(components);
            ctx.SaveChanges();
        }

        public void Gen_VehicleModel(SkdContext ctx,
            string code,
            string name,
            List<Component> components,
            List<ProductionStation> productionStations
            ) {

            var modelComponents = components.Zip(productionStations, (component, station) =>
                    new { Component = component, ProductionStation = station }
                ).Select(t => new VehicleModelComponent {
                    Component = t.Component,
                    ProductionStation = t.ProductionStation
                }).ToList();

            var vehicleModel = new VehicleModel {
                Code = code,
                Name = name,                
                ModelComponents = modelComponents
            };

            ctx.VehicleModels.Add(vehicleModel);
            ctx.SaveChanges();
        }

        public void Gen_Vehicle(SkdContext ctx,
            string vin,
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

            var lotNo = new String('X', EntityFieldLen.Vehicle_LotNo);
            var vehicleLot = new VehicleLot { LotNo = lotNo };
            ctx.VehicleLots.Add(vehicleLot);

            var vehicle = new Vehicle { 
                VIN = vin,
                Lot = vehicleLot,
                Model = vehicleModel,
                PlannedBuildAt = plannedBuildAt,
                ScanLockedAt = scanLockAt,
                VehicleComponents = vehicleComponents
            };

            ctx.Vehicles.AddRange(vehicle);
            ctx.SaveChanges();
        }
    }
}