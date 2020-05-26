
using System;
using VT.Model;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Xunit;

namespace VT.Test {
    public class ContextTest : TestBase {

        [Fact]
        public void can_add_component() {
            using (var ctx = GetAppDbContext()) {
                var component = new Component() {
                    Code = new String('X', EntityMaxLen.Component_Code),
                    Name = new String('X', EntityMaxLen.Component_Name),
                    Type = new String('X', EntityMaxLen.Component_Type),
                };

                ctx.Components.Add(component);
                ctx.SaveChanges();

                var count = ctx.Components.Count();
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public void cannot_add_duplication_component_code() {
            using (var ctx = GetAppDbContext()) {
                var component_1 = new Component() {
                    Code = "Same_Code",
                    Name = "Name1",
                    Type = "Type1"
                };

                var component_2 = new Component() {
                    Code = "Same_Code",
                    Name = "Name1",
                    Type = "Type1"
                };

                ctx.Components.Add(component_1);
                ctx.Components.Add(component_2);

                Assert.Throws<DbUpdateException>(() => ctx.SaveChanges());
            }
        }

        [Fact]
        public void cannot_add_duplication_component_name() {
            var componentName = "SameName";

            using (var ctx = GetAppDbContext()) {
                var component_1 = new Component() {
                    Code = "Code1",
                    Name = componentName,
                    Type = "Type1"
                };

                var component_2 = new Component() {
                    Code = "Code2",
                    Name = componentName,
                    Type = "Type1"
                };

                ctx.Components.Add(component_1);
                ctx.Components.Add(component_2);

                Assert.Throws<DbUpdateException>(() => ctx.SaveChanges());
            }
        }

        [Fact]
        public void can_add_vehicle_model() {
            using (var ctx = GetAppDbContext()) {
                var vehicleModel = new VehicleModel() {
                    Code = new String('X', EntityMaxLen.VehicleModel_Code),
                    Name = new String('X', EntityMaxLen.VehicleModel_Name),
                    Type = new String('X', EntityMaxLen.VehicleModel_Type),
                };

                ctx.VehicleModels.Add(vehicleModel);
                ctx.SaveChanges();

                var count = ctx.VehicleModels.Count();
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public void cannot_add_duplicate_vehicle_model_code() {
             using (var ctx = GetAppDbContext()) {
                var modelCode = new String('A', EntityMaxLen.VehicleModel_Code);
                var vehicleModel_1 = new VehicleModel() {
                    Code = modelCode,
                    Name = new String('A', EntityMaxLen.VehicleModel_Name),
                    Type = new String('A', EntityMaxLen.VehicleModel_Type),
                };

                   var vehicleModel_2 = new VehicleModel() {
                    Code = modelCode,
                    Name = new String('B', EntityMaxLen.VehicleModel_Name),
                    Type = new String('B', EntityMaxLen.VehicleModel_Type),
                };

                ctx.VehicleModels.AddRange(vehicleModel_1, vehicleModel_2);
                
                Assert.Throws<DbUpdateException>(() => ctx.SaveChanges());                        
            }
        }

         [Fact]
        public void cannot_add_duplicate_vehicle_model_name() {
             using (var ctx = GetAppDbContext()) {
                var modelName = new String('A', EntityMaxLen.Component_Name);
                var vehicleModel_1 = new VehicleModel() {
                    Code = new String('A', EntityMaxLen.VehicleModel_Code),
                    Name = modelName,
                    Type = new String('A', EntityMaxLen.VehicleModel_Type),
                };

                   var vehicleModel_2 = new VehicleModel() {
                    Code = new String('B', EntityMaxLen.VehicleModel_Code),
                    Name = modelName,
                    Type = new String('B', EntityMaxLen.VehicleModel_Type),
                };

                ctx.VehicleModels.AddRange(vehicleModel_1, vehicleModel_2);
                
                Assert.Throws<DbUpdateException>(() => ctx.SaveChanges());                        
            }
        }


        [Fact]
        public void can_add_vehicle() {
            using (var ctx = GetAppDbContext()) {
                var vehicleModel = new VehicleModel() {
                    Code = new String('X', EntityMaxLen.VehicleModel_Code),
                    Name = new String('X', EntityMaxLen.VehicleModel_Name),
                    Type = new String('X', EntityMaxLen.VehicleModel_Type),
                };

                ctx.VehicleModels.Add(vehicleModel);

                var vehicle = new Vehicle() {
                    VIN = new String('X', EntityMaxLen.Vehicle_VIN),
                    Model = vehicleModel
                };

                ctx.Vehicles.Add(vehicle);

                ctx.SaveChanges();

                var vehicleCount = ctx.VehicleModels.Count();
                Assert.Equal(1, vehicleCount);
            }
        }

        [Fact]
        public void cannot_add_vehicle_without_model() {
            using (var ctx = GetAppDbContext()) {
                var vehicle = new Vehicle() {
                    VIN = new String('X', EntityMaxLen.Vehicle_VIN),
                };

                ctx.Vehicles.Add(vehicle);

                Assert.Throws<DbUpdateException>(() => ctx.SaveChanges());
            }
        }

        [Fact]
        public void cannot_add_vehicle_duplicate_vin() {
            using (var ctx = GetAppDbContext()) {
                var vehicleModel = new VehicleModel() {
                    Code = new String('X', EntityMaxLen.VehicleModel_Code),
                    Name = new String('X', EntityMaxLen.VehicleModel_Name),
                    Type = new String('X', EntityMaxLen.VehicleModel_Type),
                };

                ctx.VehicleModels.Add(vehicleModel);

                var vehicle_1 = new Vehicle() {
                    VIN = new String('X', EntityMaxLen.Vehicle_VIN),
                    Model = vehicleModel
                };

                var vehicle_2 = new Vehicle() {
                    VIN = new String('X', EntityMaxLen.Vehicle_VIN),
                    Model = vehicleModel
                };

                ctx.Vehicles.Add(vehicle_1);
                ctx.Vehicles.Add(vehicle_2);

                Assert.Throws<DbUpdateException>(() => ctx.SaveChanges());
            }
        }
    }
}