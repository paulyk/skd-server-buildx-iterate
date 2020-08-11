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
            Assert.True(4 == components.Count);
            var vehicleModles = await ctx.VehicleModels.ToListAsync();
            Assert.True(1 == vehicleModles.Count());
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


        [Fact]
        public async Task cannot_save_component_scan_before_prerequisite_scans() {
            // setup

            var vehicle = await ctx.Vehicles
                .Include(t => t.VehicleComponents).ThenInclude(vc => vc.Component)
                .FirstAsync(t => t.VIN == vin1);

            // Last vehicle component in sequence requires sequence 1 and 2 as prerequisies
            var lastVehicleComponent = vehicle.VehicleComponents.OrderByDescending(t => t.Sequence).FirstOrDefault();
            lastVehicleComponent.PrerequisiteSequences = "1,2";
            await ctx.SaveChangesAsync();

            //test
            var dto = new ComponentScan {
                VehicleComponentId = lastVehicleComponent.Id,
                Scan1 = "",
                Scan2 = Util.RandomString(12),
            };

            var service = new ComponentScanService(ctx);
            var payload = await service.SaveComponentScan(dto);

            Assert.True(payload.Errors.Count() > 0);

            var msg ="verified prerequisite scans required";
            Assert.Contains(msg, payload.Errors.ToArray()[0].Message);
        }

        [Fact]
        public async Task can_save_component_with_prerequisite_scans_if_prerequisites_have_verified_scans() {

            var service = new ComponentScanService(ctx);

            // setup
            var preReqSeqNums = new List<int> { 1, 2 };
            var preReqSeqNums_String = preReqSeqNums.Select(t => t.ToString()).Aggregate((a, b) => a + "," + b);

            var vehicle = await ctx.Vehicles
                .Include(t => t.VehicleComponents).ThenInclude(vc => vc.Component)
                .FirstAsync(t => t.VIN == vin1);

            // Modify  Last vehicle component in sequence to requires sequence 1 and 2 as prerequisies
            var lastVehicleComponent = vehicle.VehicleComponents.OrderByDescending(t => t.Sequence).FirstOrDefault();
            
            lastVehicleComponent.PrerequisiteSequences = preReqSeqNums_String;
            await ctx.SaveChangesAsync();


            // for each of the prereq sequences save a component scan
            preReqSeqNums.ForEach(async seqUm => {
                var vcs = vehicle.VehicleComponents.FirstOrDefault(t => t.Sequence == seqUm);
                var dto = new ComponentScan {
                    VehicleComponentId = vcs.Id,
                    Scan1 = Util.RandomString(12),
                    Scan2 = Util.RandomString(12)
                };

                await service.SaveComponentScan(dto);
                // mark vehicleComponent as verfied
                vcs.ScanVerifiedAt = DateTime.UtcNow;
                // await ctx.SaveChangesAsync(); // no need due to in memory testing
            });

            //test
            var dto = new ComponentScan {
                VehicleComponentId = lastVehicleComponent.Id,
                Scan1 = "",
                Scan2 = Util.RandomString(12),
            };

            var payload = await service.SaveComponentScan(dto);

            if (payload.Errors.Count() > 0) {
                payload.Errors.ToList().ForEach(err => Console.WriteLine($"*** Error Message: {err.Message}   "));
            }
            Assert.True(payload.Errors.Count() == 0);
        }

        #region generate seed data 

        /*  setup seed data */
        private string vin1 = Util.RandomString(EntityMaxLen.Vehicle_VIN);
        private string vin2_locked = Util.RandomString(EntityMaxLen.Vehicle_VIN);

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
            var vehicles = new List<string> { vin1, vin2_locked }.ToList().Select(vin =>
              new Vehicle {
                  VIN = vin, KitNo = "123", LotNo = "123",
                  Model = vehicleModel,
                  ScanLockedAt = (vin == vin2_locked) ? DateTime.UtcNow : (DateTime?)null,
                  VehicleComponents = vehicleModel.ModelComponents.Select(mc => new VehicleComponent {
                      Component = mc.Component,
                      Sequence = mc.Sequence
                  }).ToList()
              });

            ctx.Vehicles.AddRange(vehicles);
            ctx.SaveChanges();
        }
        #endregion
    }
}
