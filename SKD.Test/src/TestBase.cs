using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SKD.Model;
using System.Linq;
using System;

namespace SKD.Test {
    public class TestBase {

        public SkdContext GetAppDbContext() {

            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<SkdContext>()
                        .UseSqlite(connection)
                        .Options;

            var ctx = new SkdContext(options);

            ctx.Database.EnsureCreated();
            return ctx;
        }

        public List<ProductionStation> Gen_ProductionStations(SkdContext ctx, params string[] codes) {
            var stationCodes = codes.Where(code => !ctx.ProductionStations.Any(t => t.Code == code)).ToList();

            var productionStations = stationCodes.ToList().Select((code, index) => new ProductionStation {
                Code = code,
                Name = $"{code} name",
                SortOrder = index + 1
            });


            ctx.ProductionStations.AddRange(productionStations);
            ctx.SaveChanges();
            return ctx.ProductionStations.ToList();
        }

        public List<Component> Gen_Components(SkdContext ctx, params string[] codes) {
            var componentCodes = codes.ToList().Where(code => !ctx.Components.Any(t => t.Code == code));

            var components = componentCodes.ToList().Select(code => new Component {
                Code = code,
                Name = $"{code} name"
            }).ToList();

            ctx.Components.AddRange(components);
            ctx.SaveChanges();
            return ctx.Components.ToList();
        }


        public VehicleModel Gen_VehicleModel(SkdContext ctx,
            string modelCode,
            List<(string componentCode,
            string stationCode)> component_stations_maps
        ) {
            Gen_Components(ctx, component_stations_maps.Select(t => t.componentCode).ToArray());
            Gen_ProductionStations(ctx, component_stations_maps.Select(t => t.stationCode).ToArray());

            var modelComponents = component_stations_maps
            .Select(map => new VehicleModelComponent {
                Component = ctx.Components.First(t => t.Code == map.componentCode),
                ProductionStation = ctx.ProductionStations.First(t => t.Code == map.stationCode)
            }).ToList();

            var vehicleModel = new VehicleModel {
                Code = modelCode,
                Name = $"{modelCode} name",
                ModelComponents = modelComponents
            };

            ctx.VehicleModels.Add(vehicleModel);
            ctx.SaveChanges();
            return vehicleModel;
        }

        public ComponentScan Gen_ComponentScan(SkdContext context, Guid vehicleComponentId) {
            var vehicleComponent = context.VehicleComponents.FirstOrDefault(t => t.Id == vehicleComponentId);
            var componentScan = new ComponentScan {
                VehicleComponentId = vehicleComponentId,
                Scan1 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry),
                Scan2 = ""
            };
            context.ComponentScans.Add(componentScan);
            context.SaveChanges();
            return componentScan;
        }

        public Vehicle Gen_Vehicle_And_Model(
            SkdContext ctx,
            string vin,
            string kitNo,
            string lotNo,
            string modelCode,
            List<(string componentCode, string stationCode)> component_stations_maps
        ) {
            var vehicleModel = Gen_VehicleModel(ctx, modelCode, component_stations_maps);
            return Gen_Vehicle_From_Model(ctx, vin, kitNo, lotNo, modelCode);
        }

        public Vehicle Gen_Vehicle_From_Model(SkdContext ctx,
            string vin,
            string kitNo,
            string lotNo,
            string modelCode
            ) {

            var vehicleModel = ctx.VehicleModels
                .Include(t => t.ModelComponents)
                .FirstOrDefault(t => t.Code == modelCode);

            var vehicleComponents = vehicleModel.ModelComponents.Select(mc => new VehicleComponent {
                ComponentId = mc.ComponentId,
                ProductionStationId = mc.ProductionStationId
            }).ToList();

            var vehicleLot = new VehicleLot { LotNo = lotNo };
            ctx.VehicleLots.Add(vehicleLot);

            var vehicle = new Vehicle {
                VIN = vin,
                Lot = vehicleLot,
                KitNo = kitNo,
                Model = vehicleModel,
                VehicleComponents = vehicleComponents
            };

            ctx.Vehicles.AddRange(vehicle);
            ctx.SaveChanges();

            return vehicle;
        }

        public Vehicle Gen_Vehicle_Amd_Model_From_Components(
            SkdContext ctx,
            List<(string componentCode, string stationCode)> component_stations_maps
        ) {

            var modelCode = Util.RandomString(EntityFieldLen.VehicleModel_Code);
            Gen_VehicleModel(
                ctx,
                modelCode: modelCode,
                component_stations_maps: component_stations_maps
              );

            // cretre vehicle based on that model
            var vehicle =  Gen_Vehicle_From_Model(ctx,
                vin: Gen_Vin(),    
                kitNo: Gen_KitNo(),            
                lotNo: Gen_LotNo(),
                modelCode: modelCode);
            return vehicle;
        }

        public void Gen_VehicleTimelineEventTypes(SkdContext ctx) {
              var eventTypes = new List<VehicleTimelineEventType> {
                new VehicleTimelineEventType {
                    Code = TimeLineEventType.CUSTOM_RECEIVED.ToString(),
                },
                new VehicleTimelineEventType {
                    Code = TimeLineEventType.PLAN_BUILD.ToString(),
                },
                new VehicleTimelineEventType {
                    Code = TimeLineEventType.BULD_COMPLETED.ToString(),
                },
                new VehicleTimelineEventType {
                    Code = TimeLineEventType.GATE_RELEASED.ToString(),
                },
                new VehicleTimelineEventType {
                    Code = TimeLineEventType.WHOLE_SALE.ToString(),
                },
            };

            var sequence = 1;
            eventTypes.ForEach(eventType => {
                eventType.Description = eventType.Code;
                eventType.Sequecne = sequence++;
            });

            ctx.VehicleTimelineEventTypes.AddRange(eventTypes);
            ctx.SaveChanges();
        }

        public string Gen_LotNo() {
            return Util.RandomString(EntityFieldLen.Vehicle_LotNo).ToUpper();
        }
        public string Gen_KitNo() {
            return Util.RandomString(EntityFieldLen.Vehicle_KitNo).ToUpper();
        }
        public string Gen_VehicleModel_Code() {
            return Util.RandomString(EntityFieldLen.VehicleModel_Code).ToUpper();
        }
        public string Gen_Vin() {
            return Util.RandomString(EntityFieldLen.Vehicle_VIN).ToUpper();
        }
        public string Gen_ComponentCode() {
            return Util.RandomString(EntityFieldLen.Component_Code).ToUpper();
        }
        public string Gen_ProductionStationCode() {
            return Util.RandomString(EntityFieldLen.ProductionStation_Code).ToUpper();
        }
    }
}