using System;
using System.Collections.Generic;
using SKD.VCS.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.VCS.Test {
    public class ComponentScanService_Test : TestBase {

        private SkdContext ctx;
        public ComponentScanService_Test() {
            ctx = GetAppDbContext();
            GenerateSeedData();
        }

        [Fact]
        public async Task seed_data_correct() {
            var componentCount = await ctx.Components.CountAsync();
            Assert.Equal(3, componentCount);

            var stationCount = await ctx.ProductionStations.CountAsync();
            Assert.Equal(3, stationCount);

            var modelCount = await ctx.VehicleModels.CountAsync();
            Assert.Equal(1, modelCount);

            var vehicleCount = await ctx.Vehicles.CountAsync();
            Assert.Equal(1, vehicleCount);
        }

        [Fact]
        public async Task can_create_component_scan() {
            var vehicleComponent = ctx.VehicleComponents.FirstOrDefault();

            var dto = new ComponentScanDTO {
                VehicleComponentId = vehicleComponent.Id,
                Scan1 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry),
                Scan2 = ""
            };

            var service = new ComponentScanService(ctx);
            var payload = await service.CreateComponentScan(dto);

            var componentScan = await ctx.ComponentScans.FirstOrDefaultAsync(t => t.Id == payload.Entity.Id);
            Assert.NotNull(componentScan);
        }

        [Fact]
        public async Task cannot_create_component_scan_if_vehicleComponentId_not_found() {

            var dto = new ComponentScanDTO {
                VehicleComponentId = Guid.NewGuid(),
                Scan1 = Util.RandomString(12),
                Scan2 = ""
            };

            var service = new ComponentScanService(ctx);
            var payload = await service.CreateComponentScan(dto);
            var errors = payload.Errors.ToList();

            Assert.True(errors.Count == 1 && errors[0].Message == "vehicle component not found");
        }

        [Fact]
        public async Task cannot_create_component_scan_if_vehicle_scan_locked() {
            // setup
            var vehicle = ctx.Vehicles.FirstOrDefault();
            vehicle.ScanLockedAt = DateTime.UtcNow;
            ctx.SaveChanges();

            vehicle = ctx.Vehicles
                .Include(t => t.VehicleComponents)
                .First(t => t.Id == vehicle.Id);

            var dto = new ComponentScanDTO {
                VehicleComponentId = vehicle.VehicleComponents.First().Id,
                Scan1 = Util.RandomString(12),
                Scan2 = ""
            };

            var service = new ComponentScanService(ctx);
            var payload = await service.CreateComponentScan(dto);

            var errors = payload.Errors.ToList();

            Assert.True(errors.Count == 1 && errors[0].Message == "vehicle scan locked");
        }

        [Fact]
        public async Task cannot_create_component_scan_if_scan1_scan2_empty() {
            var vehicleComponent = await ctx.VehicleComponents.FirstOrDefaultAsync();

            var dto = new ComponentScanDTO {
                VehicleComponentId = vehicleComponent.Id,
                Scan1 = "",
                Scan2 = ""
            };

            var service = new ComponentScanService(ctx);
            var payload = await service.CreateComponentScan(dto);

            var errors = payload.Errors.ToList();
            Assert.True(errors.Count == 1 && errors[0].Message == "scan1 and or scan2 required");
        }

        [Fact]
        public async Task cannot_create_component_scan_if_less_than_min_length() {
            var vehicleComponent = await ctx.VehicleComponents.FirstOrDefaultAsync();

            var dto = new ComponentScanDTO {
                VehicleComponentId = vehicleComponent.Id,
                Scan1 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry_Min - 1),
                Scan2 = ""
            };

            var service = new ComponentScanService(ctx);
            var payload = await service.CreateComponentScan(dto);

            var errors = payload.Errors.ToList();
            var expectedMessage = "scan entry lenth min";
            var actualMessage = errors[0].Message.Substring(0, expectedMessage.Length);
            Assert.Equal(expectedMessage, actualMessage);
        }

        [Fact]
        public async Task cannot_create_component_scan_if_no_planned_build_date() {
            var vehicleComponent = await ctx.VehicleComponents
                .Include(t => t.Vehicle)
                .FirstOrDefaultAsync();

            var vehicle = vehicleComponent.Vehicle;
            vehicle.PlannedBuildAt = null;
            await ctx.SaveChangesAsync();

            var dto = new ComponentScanDTO {
                VehicleComponentId = vehicleComponent.Id,
                Scan1 = "",
                Scan2 = ""
            };

            var service = new ComponentScanService(ctx);
            var payload = await service.CreateComponentScan(dto);

            var errors = payload.Errors.ToList();
            Assert.True(errors.Count == 1 && errors[0].Message == "vehilce planned build date required");
        }

        #region generate seed data         

        private void GenerateSeedData() {
            // components
            Gen_ProductionStations(ctx, 3);
            Gen_Components(ctx, 3);

            var components = ctx.Components.ToList();
            var productionStations = ctx.ProductionStations.ToList();

            var modelCode = "model_1";
            var modelName = "model_1_name";
            Gen_VehicleModel(
                ctx,
                code: modelCode,
                name: modelName,
                components,
                productionStations);

            Gen_Vehicle(
             ctx: ctx,
             vin: Util.RandomString(EntityFieldLen.Vehicle_VIN),
             modelCode: modelCode,
             plannedBuildAt: DateTime.UtcNow.AddDays(-2));
        }
        #endregion
    }
}
