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
            var components = await ctx.Components.ToListAsync();
            Assert.Equal(3, components.Count);
            var stations = await ctx.ProductionStations.ToListAsync();
            Assert.Equal(3, stations.Count);
            var vehicleModles = await ctx.VehicleModels.ToListAsync();
            Assert.Equal(1, vehicleModles.Count());
            Assert.Equal(3, vehicleModles[0].ModelComponents.Count);
        }

        [Fact]
        public async Task can_save_component_scan() {
            var vehicleComponent = await ctx.VehicleComponents.FirstOrDefaultAsync(t => t.Vehicle.VIN == vin1 && t.Component.Code == componentCode1);


            var dto = new ComponentScan {
                VehicleComponentId = vehicleComponent.Id,
                Scan1 = Util.RandomString(12),
                Scan2 = ""
            };

            var service = new ComponentScanService(ctx);
            var payload = await service.SaveComponentScan(dto);

            var componentScan = await ctx.ComponentScans.FirstOrDefaultAsync(t => t.Id == payload.Entity.Id);
            Assert.NotNull(componentScan);
        }

        [Fact]
        public async Task cannot_save_component_scan_if_vehicleComponentId_not_found() {

            var dto = new ComponentScan {
                VehicleComponentId = Guid.NewGuid(),
                Scan1 = Util.RandomString(12),
                Scan2 = ""
            };

            var service = new ComponentScanService(ctx);
            var payload = await service.SaveComponentScan(dto);
            var errors = payload.Errors.ToList();

            Assert.True(errors.Count == 1 && errors[0].Message == "vehicle component not found");
        }

        [Fact]
        public async Task cannot_save_component_scan_if_vehicle_scan_locked() {
            var vehicleComponent = await ctx.VehicleComponents.FirstOrDefaultAsync(t => t.Vehicle.VIN == vin2_locked && t.Component.Code == componentCode1);

            var dto = new ComponentScan {
                VehicleComponentId = vehicleComponent.Id,
                Scan1 = Util.RandomString(12),
                Scan2 = ""
            };

            var service = new ComponentScanService(ctx);
            var payload = await service.SaveComponentScan(dto);

            var errors = payload.Errors.ToList();

            Assert.True(errors.Count == 1 && errors[0].Message == "vehicle locked, scans not allowed");
        }

        [Fact]
        public async Task cannot_save_component_scan_if_scan1_scan2_empty() {
            var vehicleComponent = await ctx.VehicleComponents.FirstOrDefaultAsync(t => t.Vehicle.VIN == vin1 && t.Component.Code == componentCode1);

            var dto = new ComponentScan {
                VehicleComponentId = vehicleComponent.Id,
                Scan1 = "",
                Scan2 = ""
            };

            var service = new ComponentScanService(ctx);
            var payload = await service.SaveComponentScan(dto);

            var errors = payload.Errors.ToList();
            Assert.True(errors.Count == 1 && errors[0].Message == "scan1 and or scan2 required");
        }


        #region generate seed data 

        /*  setup seed data */
        private string vin1 = Util.RandomString(EntityMaxLen.Vehicle_VIN);
        private string vin2_locked = Util.RandomString(EntityMaxLen.Vehicle_VIN);



        private string stationCode1 = "STATIONE_1";
        private string stationCode2 = "STATIONE_2";
        private string stationCode3 = "STATIONE_3";

        private string componentCode1 = "COMP_1";
        private string componentCode2 = "COMP_2";
        private string componentCode3 = "COMP_3";
        

        private void GenerateSeedData() {
            // components
            var componentCodes = new string[] { componentCode1, componentCode2, componentCode3 };
            var components = componentCodes.Select(code => new Component { Code = code, Name = code }).ToList(); // ToList() to prevent UNIQUE constraint failed: component.Name'
            ctx.Components.AddRange(components);

            // production stations
            var productionStationCodes = new string[] { stationCode1, stationCode2, stationCode3  };
            var productionStations = productionStationCodes.Select(code => new ProductionStation { Code = code, Name = code }).ToList(); // ToList() to prevent UNIQUE constraint failed: component.Name'
            ctx.ProductionStations.AddRange(productionStations);
            ctx.SaveChanges();

            components = ctx.Components.ToList();
            productionStations = ctx.ProductionStations.ToList();
            var zipped = components.Zip(productionStations, (component, station) => new { Component = component, ProductionStation = station});      
            var modelComponents = zipped.Select(t => new VehicleModelComponent {
                Component = t.Component,
                ProductionStation = t.ProductionStation
            }).ToList();

            var vehicleModel = new VehicleModel {
                Code = "Model Code",
                Name = "Model Name",
                Type = "Model Type",
                ModelComponents = modelComponents
            };

            ctx.VehicleModels.Add(vehicleModel);

            // vehicles
            var vehicles = new List<string> { vin1, vin2_locked }.ToList().Select(vin =>
              new Vehicle {
                  VIN = vin, KitNo = "123", LotNo = "123",
                  Model = vehicleModel,
                  ScanLockedAt = (vin == vin2_locked) ? DateTime.UtcNow : (DateTime?)null,
                  VehicleComponents = vehicleModel.ModelComponents.Select(mc => new VehicleComponent() { 
                      Component = mc.Component, 
                      ProductionStation = mc.ProductionStation
                  }).ToList()
              });

            ctx.Vehicles.AddRange(vehicles);
            ctx.SaveChanges();
        }
        #endregion
    }
}
