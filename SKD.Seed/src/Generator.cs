using SKD.Model;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;

namespace SKD.Seed {
    internal class Generator {
        private SkdContext ctx;

        public Generator(SkdContext ctx) {
            this.ctx = ctx;
        }

        public async Task DroCreateDb() {
            await ctx.Database.EnsureDeletedAsync();
            Console.WriteLine("Dropped database");
            await ctx.Database.MigrateAsync();
            Console.WriteLine("Created database");
        }

        public async Task Seed_VehicleModelComponents(ICollection<VehicleModelComponent_Seed_DTO> vehicleModelComponentData) {

            // vehicle model components
            var components = vehicleModelComponentData.ToList();
            var vehicleModelComponents = new List<VehicleModelComponent>();

            foreach (var item in vehicleModelComponentData) {
                var component = await ctx.Components.FirstOrDefaultAsync(c => c.Code == item.componentCode);
                var model = await ctx.VehicleModels.FirstOrDefaultAsync(m => m.Code == item.modelCode);

                vehicleModelComponents.Add(new VehicleModelComponent() {
                    Component = component,
                    VehicleModel = model,
                    Sequence = item.sequence,
                    PrerequisiteSequences = item.prerequisite,
                    CreatedAt = SeedUtil.RandomDateTime(DateTime.UtcNow)
                });
            }

            foreach (var vmc in vehicleModelComponents) {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)) {
                    var existing = await ctx.VehicleModelComponents
                        .Where(x => x.VehicleModel.Code == vmc.VehicleModel.Code && x.Component.Code == vmc.Component.Code)
                        .FirstOrDefaultAsync();

                    if (existing != null) {
                        Console.WriteLine($"Duplicate model component entry: {existing.VehicleModel.Name}  {existing.Component.Code}");
                        
                    } else {
                        ctx.VehicleModelComponents.Add(vmc);
                        await ctx.SaveChangesAsync();
                    }
                    scope.Complete();
                }
            }

            var count = await ctx.VehicleModelComponents.CountAsync();
            Console.WriteLine($"Added {count} vehicle model components ");
        }

        public async Task Seed_Vehicles(ICollection<Vehicle_Seed_DTO> vehicleData) {

            if (await ctx.VehicleModels.CountAsync() == 0) {
                throw new Exception("Vehicle models must be seeded before vheicles");
            }

            foreach (var entry in vehicleData.ToList()) {
                var vehicle = new Vehicle(){
                    VIN = entry.vin,
                    KitNo = entry.kitNo,
                    LotNo = entry.lotNo,
                    Model = ctx.VehicleModels.First(m => m.Code == entry.modelId),
                    CreatedAt = SeedUtil.RandomDateTime(DateTime.UtcNow)
                };

                ctx.Vehicles.Add(vehicle);

                var modelComponents = ctx.VehicleModelComponents.Where(t => t.VehicleModelId == vehicle.ModelId).ToList();
                Console.WriteLine($"{vehicle.Model.Name} has {modelComponents.Count} components");

                foreach(var modelComponent in modelComponents) {
                    vehicle.VehicleComponents.Add(new VehicleComponent() {
                        Component = modelComponent.Component,
                        ComponentId = modelComponent.ComponentId,
                        Sequence = modelComponent.Sequence,
                        CreatedAt = vehicle.CreatedAt
                    });
                }

                Console.WriteLine($"  Added  {vehicle.VehicleComponents.Count} children");   
                Console.WriteLine("--------");
                Console.WriteLine(" ");
                await ctx.SaveChangesAsync();     
            }
            // vehicles        
            Console.WriteLine($"Added {await ctx.Vehicles.CountAsync()} vehicles");
        }

        public async Task Seed_Components(ICollection<Component_Seed_DTO> componentData) {
            var components = componentData.ToList().Select(x => new Component() {
                Code = x.code,
                Name = x.name,
                CreatedAt = SeedUtil.RandomDateTime(DateTime.UtcNow)
            });

            ctx.Components.AddRange(components);
            await ctx.SaveChangesAsync();
            
            Console.WriteLine($"Added {ctx.Components.Count()} components");
        }

        public async Task Seed_VehicleModels(ICollection<VehicleModel_Seed_DTO> vehicleModelData) {
            var vehicleModels = vehicleModelData.ToList().Select(x => new VehicleModel() {
                Code = x.code,
                Name = x.name,
            });

            ctx.VehicleModels.AddRange(vehicleModels);
            await ctx.SaveChangesAsync();
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