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
        public void cannot_scan_if_vin_not_found() {
        }

        private string vin1 = Util.RandomString(EntityMaxLen.Vehicle_VIN);
        private string vin2 = Util.RandomString(EntityMaxLen.Vehicle_VIN);

        private void GenerateSeedData() {

            var components = new List<Component> {
                new Component() { Code = "COMP1", Name = "Component name 1" },
                new Component() { Code = "COMP2", Name = "Component name 2" },
                new Component() { Code = "COMP3", Name = "Component name 2" },
            };
            ctx.Components.AddRange(components);

            var vehicleModel = new VehicleModel {
                Code = "model1",
                Name = "Model 1",
                Type = "Type1",
            };
            var seqNum = 1;
            components.ForEach(component => {
                vehicleModel.ModelComponents.Add(new VehicleModelComponent {
                    Component = component,
                    Sequence = seqNum++
                });
            });
            ctx.VehicleModels.Add(vehicleModel);



            var vehicles = new List<Vehicle> {
              new Vehicle { 
                VIN = vin1, KitNo = "123", LotNo = "123",
                VehicleComponents = vehicleModel.ModelComponents.Select(mc => new VehicleComponent {
                  Component = mc.Component,
                  Sequence = mc.Sequence
                }).ToList()
              },
              new Vehicle { 
                VIN = vin2, KitNo = "123", LotNo = "123",
              },
            };

            ctx.Vehicles.AddRange(vehicles);
            ctx.SaveChanges();
        }
    }
}
