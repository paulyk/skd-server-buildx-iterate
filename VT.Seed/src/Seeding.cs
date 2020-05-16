using VT.Model;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace VT.Seed {
    public class Seeding {
        private AppDbContext ctx;

        public Seeding(AppDbContext ctx) {
            this.ctx = ctx;
        }

        public void DroCreateDb() {
            ctx.Database.EnsureDeleted();
            Console.WriteLine("Dropped database");
            ctx.Database.EnsureCreated();
            Console.WriteLine("Created database");
        }        

        public void Seed_VehicleModelComponents(ICollection<VehicleModelComponent_Seed_DTO> vehicleModelComponentData) {

            // vehicle model components
            var vehicleModelComponents = vehicleModelComponentData.ToList().Select(x => new VehicleModelComponent() {
                Component = ctx.Components.First(c => c.Code == x.componentCode),
                VehicleModel = ctx.VehicleModels.First(m => m.Code == x.modelCode),
                Sequence = x.sequence
            }).ToList();

            vehicleModelComponents.ForEach(vmc => {
                var existing = ctx.VehicleModelComponents
                    .Where(x => x.VehicleModel.Code == vmc.VehicleModel.Code && x.Component.Code == vmc.Component.Code)
                    .FirstOrDefault();

                if (existing != null) {
                    existing.Quantity += 1;
                    ctx.SaveChanges();
                } else {
                    ctx.VehicleModelComponents.Add(vmc);
                    ctx.SaveChanges();
                }
            });
            Console.WriteLine($"Added {ctx.VehicleModelComponents.Count()} vehicle model components ");
        }

        public void Seed_Vehicles(ICollection<Vehicle_Seed_DTO> vehicleData) {
            // vehicles
            var vehicles = vehicleData.ToList().Select(x => new Vehicle() {
                VIN = x.vin,
                KitNo = x.kitNo,
                LotNo = x.lotNo,
                Model = ctx.VehicleModels.First(m => m.Code == x.modelId)
            });

            ctx.Vehicles.AddRange(vehicles);
            ctx.SaveChanges();
            Console.WriteLine($"Added {vehicles.Count()} vehicles");
        }

        public void Seed_Components(ICollection<Component_Seed_DTO> componentData) {
            var components = componentData.ToList().Select(x => new Component() {
                Code = x.code,                
                Name = x.name,
                Type = x.type
            });

            ctx.Components.AddRange(components);
            ctx.SaveChanges();
            Console.WriteLine($"Added {ctx.Components.Count()} components");
        }

        public void Seed_VehicleModels(ICollection<VehicleModel_Seed_DTO> vehicleModelData) {
            var vehicleModels = vehicleModelData.ToList().Select(x => new VehicleModel() {
                Code = x.code,
                Name = x.name,
            });

            ctx.VehicleModels.AddRange(vehicleModels);
            ctx.SaveChanges();
            Console.WriteLine($"Added {ctx.VehicleModels.Count()} vehicle models");
        }

        public void CheckDuplicates(SeedData seedData) {

            var duplicateVehileModelCode = seedData.VehicleModel_SeedData.ToList().GroupBy(x => x.code).Select(g => new {
                Code = g.Key,
                Count = g.Count()
            }).Any(g => g.Count > 1);

            if (duplicateVehileModelCode) {
                throw new Exception("Found duplicate vehilce model code");
            }

            var duplicateVehicleModelName = seedData.VehicleModel_SeedData.ToList().GroupBy(x => x.name).Select(g => new {
                Name = g.Key,
                Count = g.Count()
            }).Any(g => g.Count > 1);

            if (duplicateVehicleModelName) {
                throw new Exception("Found duplicate vehilce model name");
            }

            var duplicateComponentCode = seedData.Component_SeedData.ToList().GroupBy(x => x.code).Select(g => new {
                Name = g.Key,
                Count = g.Count()
            }).Any(g => g.Count > 1);


            if (duplicateComponentCode) {
                throw new Exception("duplicate component code");
            }

            var duplicateVehicleModelComponents = seedData.VehicleModelComponent_SeedData.ToList().GroupBy(x => new { x.modelCode, x.componentCode }).Select(g => new {
                Name = g.Key,
                Count = g.Count()
            }).Any(g => g.Count > 1);

            if (duplicateComponentCode) {
                throw new Exception("duplicate vehicle model components code");
            }
        }
    }
}