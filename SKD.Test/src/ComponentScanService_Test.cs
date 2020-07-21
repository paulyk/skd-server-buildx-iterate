using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class ComponentScanService_Test : TestBase {

        private SkdContext ctx;
        public ComponentScanService_Test() {
            ctx = GetAppDbContext();
            GenerateSeedData();
        }

        [Fact]
        public async Task seed_data_correct() {
            var components = await ctx.Components.ToListAsync();
            Assert.Equal(4, components.Count);
            var vehicleModles = await ctx.VehicleModels.ToListAsync();
            Assert.Equal(1, vehicleModles.Count());
        }

        [Fact]
        public async Task can_save_component_scan() {
            var dto = new CreateComponentScan_DTO {
                VIN = vin_in_progress,
                ComponentCode = componentCode1,
                Scan1 = Util.RandomString(12),
                Scan2 = ""
            };

            var service = new ComponentScanService(ctx);
            var payload = await service.SaveComponentScan(dto);

            var componentScan = await ctx.ComponentScans.FirstOrDefaultAsync(t => t.Id == payload.Entity.Id);
            Assert.NotNull(componentScan);
        }


        [Fact]
        public async Task cannot_save_scan_if_vin_not_found() {
            var dto = new CreateComponentScan_DTO {
                VIN = "VVVVV" + Util.RandomString(EntityMaxLen.Vehicle_VIN - 5),
                ComponentCode = componentCode1,
                Scan1 = Util.RandomString(12),
                Scan2 = ""
            };

            var service = new ComponentScanService(ctx);
            var payload = await service.SaveComponentScan(dto);

            var errors = payload.Errors.ToList();

            Assert.True(errors.Count == 1 && errors[0].Message == "vin not found");
        }

        [Fact]
        public async Task cannot_save_scan_if_component_scan_locked() {
            var dto = new CreateComponentScan_DTO {
                VIN = vin_locked,
                ComponentCode = componentCode1,
                Scan1 = Util.RandomString(12),
                Scan2 = ""
            };

            var service = new ComponentScanService(ctx);
            var payload = await service.SaveComponentScan(dto);

            var errors = payload.Errors.ToList();

            Assert.True(errors.Count == 1 && errors[0].Message == "vin component scan locked");
        }

        [Fact]
        public async Task cannot_save_scan_if_component_code_not_found() {
            var randomComponentCode = Util.RandomString(EntityMaxLen.Component_Code);
            var dto = new CreateComponentScan_DTO {
                VIN = vin_in_progress,
                ComponentCode = randomComponentCode,
                Scan1 = Util.RandomString(12),
                Scan2 = ""
            };

            var service = new ComponentScanService(ctx);
            var payload = await service.SaveComponentScan(dto);

            var errors = payload.Errors.ToList();

            Assert.True(errors.Count == 1 && errors[0].Message == "component code not found");
        }

        [Fact]
        public async Task cannot_save_scan_if_component_not_in_vehicle_components() {
            var randomComponentCode = Util.RandomString(EntityMaxLen.Component_Code);
            var dto = new CreateComponentScan_DTO {
                VIN = vin_in_progress,
                ComponentCode = unused_componentCode,
                Scan1 = Util.RandomString(12),
                Scan2 = ""
            };

            var service = new ComponentScanService(ctx);
            var payload = await service.SaveComponentScan(dto);

            var errors = payload.Errors.ToList();

            Assert.True(errors.Count == 1 && errors[0].Message == "component code not used in this vehicle");
        }

        [Fact]
        public async Task cannot_add_if_sequence_incorrect() {
            // use existing componetn but wrong sequence number
        }

        private string vin_in_progress = Util.RandomString(EntityMaxLen.Vehicle_VIN);
        private string vin_locked = Util.RandomString(EntityMaxLen.Vehicle_VIN);

        private string componentCode1 = "COMP_1";
        private string componentCode2 = "COMP_2";
        private string componentCode3 = "COMP_3";
        private string unused_componentCode = "COMP_4";

        private void GenerateSeedData() {
            //components
            var componentCodes = new string[] { componentCode1, componentCode2, componentCode3, unused_componentCode };
            var components = componentCodes.Select(code => new Component { Code = code, Name = code }).ToList(); // ToList() to prevent UNIQUE constraint failed: component.Name'
            ctx.Components.AddRange(components);

            // vehicle model            
            var seqNum = 1;
            var modelComponents = components.Where(t => t.Code != unused_componentCode).ToList().Select(component =>
                new VehicleModelComponent {
                    Component = component,
                    Sequence = seqNum++
                }).ToList();

            var vehicleModel = new VehicleModel {
                Code = "Model Code",
                Name = "Model Name",
                Type = "Model Type",
                ModelComponents = modelComponents
            };

            ctx.VehicleModels.Add(vehicleModel);

            // vehicles
            var vehicles = new List<string> { vin_in_progress, vin_locked }.ToList().Select(vin =>
              new Vehicle {
                  VIN = vin, KitNo = "123", LotNo = "123",
                  Model = vehicleModel,
                  ComponentScanLockedAt = (vin == vin_locked) ? DateTime.UtcNow : (DateTime?)null,
                  VehicleComponents = vehicleModel.ModelComponents.Select(mc => new VehicleComponent {
                      Component = mc.Component,
                      Sequence = mc.Sequence
                  }).ToList()
              });

            ctx.Vehicles.AddRange(vehicles);
            ctx.SaveChanges();
        }
    }
}
