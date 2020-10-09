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
            vehicle.ScanCompleteAt = DateTime.UtcNow;
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
            var expectedMessage = "scan entry length min";
            var actualMessage = errors[0].Message.Substring(0, expectedMessage.Length);
            Assert.Equal(expectedMessage, actualMessage);
        }

        [Fact]
        public async Task cannot_create_component_scan_for_vehicle_component_if_one_already_exists() {
            var vehicleComponent = await ctx.VehicleComponents.FirstOrDefaultAsync();

            var dto_1 = new ComponentScanDTO {
                VehicleComponentId = vehicleComponent.Id,
                Scan1 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry),
                Scan2 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry)
            };

            var dto_2 = new ComponentScanDTO {
                VehicleComponentId = vehicleComponent.Id,
                Scan1 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry),
                Scan2 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry)
            };

            var service = new ComponentScanService(ctx);
            var payload_1 = await service.CreateComponentScan(dto_1);

            Assert.True(payload_1.Errors.Count == 0);

            var payload_2 = await service.CreateComponentScan(dto_2);

            Assert.True(payload_2.Errors.Count > 0);

            var message = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
            Assert.Equal("Existing scan found", message);
        }

        [Fact]
        public async Task create_component_scan_swaps_if_scan1_empty() {
            var vehicleComponent = await ctx.VehicleComponents.FirstOrDefaultAsync();

            var scan1 = "";
            var scan2 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry);

            var dto = new ComponentScanDTO {
                VehicleComponentId = vehicleComponent.Id,
                Scan1 = scan1,
                Scan2 = scan2
            };

            var before_count = await ctx.ComponentScans.CountAsync();

            var service = new ComponentScanService(ctx);
            var payload = await service.CreateComponentScan(dto);

            var after_count = await ctx.ComponentScans.CountAsync();
            Assert.Equal(before_count + 1, after_count);

            var componentScan = await ctx.ComponentScans.FirstAsync(t => t.VehicleComponentId == vehicleComponent.Id);
            Assert.Equal(componentScan.Scan1, scan2);
            Assert.True(String.IsNullOrEmpty(componentScan.Scan2));
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
                Scan1 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry),
                Scan2 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry)
            };

            var service = new ComponentScanService(ctx);
            var payload = await service.CreateComponentScan(dto);

            var errors = payload.Errors.ToList();
            Assert.True(errors.Count == 1 && errors[0].Message == "vehicle planned build date required");
        }

        

        #region generate seed data         

        private void GenerateSeedData() {
            // components
            Gen_ProductionStations(ctx, "station_1", "station_2");
            Gen_Components(ctx, "component_1", "component_2");

            var components = ctx.Components.ToList();
            var productionStations = ctx.ProductionStations.ToList();

            var modelCode = "model_1";
            Gen_VehicleModel(
                ctx,
                code: modelCode,
                component_stations_maps: new List<(string, string)> {
                    ("component_1", "station_1"),
                    ("component_2", "station_2"),
                }
              );

            Gen_Vehicle(
             ctx: ctx,
             vin: Util.RandomString(EntityFieldLen.Vehicle_VIN),
             modelCode: modelCode,
             plannedBuildAt: DateTime.UtcNow.AddDays(-2));
        }
        #endregion
    }
}
