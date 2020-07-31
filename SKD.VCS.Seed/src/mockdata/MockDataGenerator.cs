using SKD.VCS.Model;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;

namespace SKD.VCS.Seed {
    internal class MockDataGenerator {
        private SkdContext ctx;

        public MockDataGenerator(SkdContext ctx) {
            this.ctx = ctx;
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
                    CreatedAt = Util.RandomDateTime(DateTime.UtcNow)
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

            var index = 1;
            foreach (var entry in vehicleData.ToList()) {
                var vehicle = new Vehicle() {
                    VIN = entry.vin,
                    KitNo = entry.kitNo,
                    LotNo = entry.lotNo,
                    Model = ctx.VehicleModels.First(m => m.Code == entry.modelId),
                    CreatedAt = Util.RandomDateTime(DateTime.UtcNow)
                };

                ctx.Vehicles.Add(vehicle);

                var modelComponents = ctx.VehicleModelComponents.Where(t => t.VehicleModelId == vehicle.ModelId).ToList();

                foreach (var modelComponent in modelComponents) {
                    var vehicleComponent = new VehicleComponent() {
                        Component = modelComponent.Component,
                        ComponentId = modelComponent.ComponentId,
                        Sequence = modelComponent.Sequence,
                        PrerequisiteSequences = modelComponent.PrerequisiteSequences,
                        CreatedAt = vehicle.CreatedAt
                    };

                    vehicle.VehicleComponents.Add(vehicleComponent);
                }

                Console.WriteLine($"  Added  {vehicle.VehicleComponents.Count} children");
                Console.WriteLine("--------");
                Console.WriteLine(" ");
                await ctx.SaveChangesAsync();
                index++;
            }
            // vehicles        
            Console.WriteLine($"Added {await ctx.Vehicles.CountAsync()} vehicles");
        }

        public void Seed_VehicleComponentScan(Vehicle vehicle) {
            var date =vehicle.CreatedAt.AddDays(4);
            var i = 0;
            foreach(var vc in vehicle.VehicleComponents) {
                var componentScan = new ComponentScan {
                    Scan1 = Util.RandomString(30),
                    Scan2 = i++ % 3 == 0 ? Util.RandomString(15) : "",
                    CreatedAt = date
                };
                vc.ComponentScans.Add(componentScan);

                date = date.AddDays(1);
            };
        }

        public async Task Seed_Components(ICollection<Component_Seed_DTO> componentData) {
            var components = componentData.ToList().Select(x => new Component() {
                Code = x.code,
                Name = x.name,
                CreatedAt = Util.RandomDateTime(DateTime.UtcNow)
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
    }
}