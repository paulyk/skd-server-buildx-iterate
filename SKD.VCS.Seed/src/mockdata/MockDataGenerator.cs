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

        public async Task Seed_VehicleModelComponents(ICollection<CmponentStation_McckData_DTO> componentStationMapping) {

            var vehicleModels = await ctx.VehicleModels.ToListAsync();
            var components = await ctx.Components.ToListAsync();
            var stations = await ctx.ProductionStations.ToListAsync();

            // check 

            componentStationMapping.ToList().ForEach(m => {
                var existingComponent = components.FirstOrDefault(c => c.Code == m.componentCode);
                if (existingComponent == null) {
                    throw new Exception($"comopnent code not found {m.componentCode}");
                }
                var existingStation = stations.FirstOrDefault(c => c.Code == m.stationCode);
                if (existingStation == null) {
                    throw new Exception($"comopnent code not found {m.stationCode}");
                }
            });

            foreach (var model in vehicleModels) {
                foreach(var mapping in componentStationMapping) {
                    var component = components.First(t => t.Code == mapping.componentCode);
                    var station = stations.First(t => t.Code == mapping.stationCode);

                    var modelComponent = new VehicleModelComponent {
                        Component = component,
                        ProductionStation = station
                    };
                    model.ModelComponents.Add(modelComponent);
                }             
            }

            await ctx.SaveChangesAsync();
            var count = await ctx.VehicleModelComponents.CountAsync();
            Console.WriteLine($"Added {count} vehicle model components ");
        }

        public async Task Seed_Vehicles(ICollection<Vehicle_MockData_DTO> vehicleData) {

            if (await ctx.VehicleModels.CountAsync() == 0) {
                throw new Exception("Vehicle models must be seeded before vheicles");
            }

            var index = 1;
            
            foreach (var entry in vehicleData) {
                var vehicleModel = ctx.VehicleModels.First(m => m.Code == entry.modelCode);
                var vehicle = new Vehicle() {
                    VIN = entry.vin,
                    Model = vehicleModel,
                    PlannedBuildAt = index % 2 == 0 
                        ? DateTime.UtcNow.Date
                        : (DateTime?)null,
                    CreatedAt = Util.RandomDateTime(DateTime.UtcNow.Date.AddMonths(-1))
                };

                ctx.Vehicles.Add(vehicle);

                var modelComponents = ctx.VehicleModelComponents.Where(t => t.VehicleModelId == vehicle.ModelId).ToList();

                foreach (var modelComponent in modelComponents) {
                    var vehicleComponent = new VehicleComponent() {
                        Component = modelComponent.Component,
                        ComponentId = modelComponent.ComponentId,
                        ProductionStationId = modelComponent.ProductionStationId,
                        ProductionStation = modelComponent.ProductionStation,
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
            var date = vehicle.CreatedAt.AddDays(4);
            var i = 0;
            foreach (var vc in vehicle.VehicleComponents) {
                var componentScan = new ComponentScan {
                    Scan1 = Util.RandomString(30),
                    Scan2 = i++ % 3 == 0 ? Util.RandomString(15) : "",
                    CreatedAt = date
                };
                vc.ComponentScans.Add(componentScan);

                date = date.AddDays(1);
            };
        }

        public async Task Seed_ProductionStations(ICollection<ProductionStation_Mock_DTO> data) {
            var stations = data.ToList().Select(x => new ProductionStation() {
                Code = x.code,
                Name = x.name,
                SortOrder = x.sortOrder,
                CreatedAt = Util.RandomDateTime(DateTime.UtcNow)
            });

            ctx.ProductionStations.AddRange(stations);
            await ctx.SaveChangesAsync();

            Console.WriteLine($"Added {ctx.ProductionStations.Count()} production stations");
        }

        public async Task Seed_Components(ICollection<Component_MockData_DTO> componentData) {
            var components = componentData.ToList().Select(x => new Component() {
                Code = x.code,
                Name = x.name,
                CreatedAt = Util.RandomDateTime(DateTime.UtcNow)
            });

            ctx.Components.AddRange(components);
            await ctx.SaveChangesAsync();

            Console.WriteLine($"Added {ctx.Components.Count()} components");
        }

        public async Task Seed_VehicleModels(ICollection<VehicleModel_MockData_DTO> vehicleModelData) {
            var vehicleModels = vehicleModelData.ToList().Select(x => new VehicleModel() {
                Code = x.code,
                Name = x.name,
            }).ToList();        

            ctx.VehicleModels.AddRange(vehicleModels);
            await ctx.SaveChangesAsync();
            Console.WriteLine($"Added {ctx.VehicleModels.Count()} vehicle models");
        }
    }
}