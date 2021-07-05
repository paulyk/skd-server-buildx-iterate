using System;
using SKD.Model;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Xunit;
using System.Collections.Generic;

namespace SKD.Test {
    public class ContextTest : TestBase {

        [Fact]
        public void can_add_component() {
            using (var ctx = GetAppDbContext()) {
                // setup
                var component = new Component() {
                    Code = new String('X', EntityFieldLen.Component_Code),
                    Name = new String('X', EntityFieldLen.Component_Name)
                };

                ctx.Components.Add(component);

                // test
                ctx.SaveChanges();

                // assert
                var count = ctx.Components.Count();
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public void cannot_add_duplication_component_code() {
            using (var ctx = GetAppDbContext()) {
                // setup
                var component_1 = new Component() {
                    Code = "Same_Code",
                    Name = "Name1",
                };

                var component_2 = new Component() {
                    Code = "Same_Code",
                    Name = "Name1",
                };

                ctx.Components.Add(component_1);
                ctx.Components.Add(component_2);

                // test + assert
                Assert.Throws<DbUpdateException>(() => ctx.SaveChanges());
            }
        }

        [Fact]
        public void cannot_add_duplication_component_name() {
            var componentName = "SameName";

            using (var ctx = GetAppDbContext()) {
                // setup
                var component_1 = new Component() {
                    Code = "Code1",
                    Name = componentName,
                };

                var component_2 = new Component() {
                    Code = "Code2",
                    Name = componentName,
                };

                ctx.Components.Add(component_1);
                ctx.Components.Add(component_2);

                // test + assert
                Assert.Throws<DbUpdateException>(() => ctx.SaveChanges());
            }

        }

        [Fact]
        public void can_add_vehicle_model() {
            using (var ctx = GetAppDbContext()) {
                // setup
                var vehicleModel = new VehicleModel() {
                    Code = new String('X', EntityFieldLen.VehicleModel_Code),
                    Name = new String('X', EntityFieldLen.VehicleModel_Description),
                    ModelYear = new String('X', EntityFieldLen.VehicleModel_ModelYear),
                };

                ctx.VehicleModels.Add(vehicleModel);
                // test
                ctx.SaveChanges();

                // assert
                var count = ctx.VehicleModels.Count();
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public void submit_model_input_twice_has_no_side_effect() {
            using (var ctx = GetAppDbContext()) {
                // setup
                var modelCode = new String('A', EntityFieldLen.VehicleModel_Code);
                var vehicleModel_1 = new VehicleModel() {
                    Code = modelCode,
                    Name = new String('A', EntityFieldLen.VehicleModel_Description),
                    ModelYear = new String('A', EntityFieldLen.VehicleModel_ModelYear),
                };

                var vehicleModel_2 = new VehicleModel() {
                    Code = modelCode,
                    Name = new String('B', EntityFieldLen.VehicleModel_Description),
                    ModelYear = new String('B', EntityFieldLen.VehicleModel_ModelYear),
                };

                ctx.VehicleModels.AddRange(vehicleModel_1, vehicleModel_2);

                // test + assert
                Assert.Throws<DbUpdateException>(() => ctx.SaveChanges());
            }
        }

        [Fact]
        public void can_add_duplicate_vehicle_model_name() {
            using (var ctx = GetAppDbContext()) {
                // setup
                var modelName = new String('A', EntityFieldLen.Component_Name);
                var vehicleModel_1 = new VehicleModel() {
                    Code = new String('A', EntityFieldLen.VehicleModel_Code),
                    Name = modelName,
                    ModelYear = new String('A', EntityFieldLen.VehicleModel_ModelYear),
                };

                var vehicleModel_2 = new VehicleModel() {
                    Code = new String('B', EntityFieldLen.VehicleModel_Code),
                    Name = modelName,
                    ModelYear = new String('B', EntityFieldLen.VehicleModel_ModelYear),
                };

                ctx.VehicleModels.AddRange(vehicleModel_1, vehicleModel_2);
                ctx.SaveChanges();

                var count = ctx.VehicleModels.Count(t => t.Name == modelName);
                Assert.Equal(2, count);
            }
        }


        [Fact]
        public void can_add_kit() {
            using (var ctx = GetAppDbContext()) {
                // setup
                var vehicleModel = new VehicleModel() {
                    Code = new String('X', EntityFieldLen.VehicleModel_Code),
                    Name = new String('X', EntityFieldLen.VehicleModel_Description),
                    ModelYear = new String('X', EntityFieldLen.VehicleModel_ModelYear),
                };
                ctx.VehicleModels.Add(vehicleModel);
                ctx.SaveChanges();

                // plant
                var plant = new Plant { 
                    Code = Gen_PlantCode(), 
                    PartnerPlantCode = Gen_PartnerPLantCode(), 
                    PartnerPlantType = Gen_PartnerPlantType() 
                };
                ctx.Plants.Add(plant);

                // bom
                var bom = new Bom { Sequence = 1, Plant = plant };
                ctx.Boms.Add(bom);

                // lot
                var model = ctx.VehicleModels.First();
                var lotNo = new String('X', EntityFieldLen.LotNo);
                var lot = new Lot {
                    LotNo = Gen_LotNo(model.Code, 1),
                    Model = model,
                    Bom = bom,
                    Plant = plant
                };
                ctx.Lots.Add(lot);

                // kit 
                var kit = new Kit() {
                    VIN = new String('X', EntityFieldLen.VIN),
                    Lot = lot,
                };

                ctx.Kits.Add(kit);

                // test
                ctx.SaveChanges();

                // assert
                var kitCount = ctx.VehicleModels.Count();
                Assert.Equal(1, kitCount);
            }
        }

        [Fact]
        public void cannot_add_vehicle_without_model() {
            using (var ctx = GetAppDbContext()) {
                // setup
                var vehicle = new Kit() {
                    VIN = new String('X', EntityFieldLen.VIN),
                };

                ctx.Kits.Add(vehicle);

                // test + assert
                Assert.Throws<DbUpdateException>(() => ctx.SaveChanges());
            }
        }

        [Fact]
        public void cannot_add_vehicle_duplicate_vin() {
            using (var ctx = GetAppDbContext()) {
                // setup
                var vehicleModel = new VehicleModel() {
                    Code = new String('X', EntityFieldLen.VehicleModel_Code),
                    Name = new String('X', EntityFieldLen.VehicleModel_Description),
                    ModelYear = new String('X', EntityFieldLen.VehicleModel_ModelYear),
                };

                ctx.VehicleModels.Add(vehicleModel);

                var vehicle_1 = new Kit() {
                    VIN = new String('X', EntityFieldLen.VIN),
                };

                var vehicle_2 = new Kit() {
                    VIN = new String('X', EntityFieldLen.VIN),
                };

                ctx.Kits.AddRange(vehicle_1, vehicle_2);

                // test + assert
                Assert.Throws<DbUpdateException>(() => ctx.SaveChanges());
            }
        }

        [Fact]
        public void can_add_parts() {
            using (var ctx = GetAppDbContext()) {
                var parts = new List<Part> {
                    new Part { PartNo = "p1", OriginalPartNo = "p1 -", PartDesc = "p1 desc"},
                    new Part { PartNo = "p2", OriginalPartNo = "p2 -", PartDesc = "p2 desc"},
                };

                ctx.Parts.AddRange(parts);
                var before_count = ctx.Parts.Count();
                Assert.Equal(0, before_count);

                ctx.SaveChanges();

                var after_count = ctx.Parts.Count();
                Assert.Equal(parts.Count, after_count);
            }
        }
    }
}