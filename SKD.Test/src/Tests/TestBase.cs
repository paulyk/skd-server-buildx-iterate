using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SKD.Model;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace SKD.Test {

    public class TestBase {

        protected SkdContext ctx;
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

        ///<summary>
        /// Test Seed Data: 
        /// Component, Production Stationes, Timeline events
        /// Plant, Bom, 
        /// Vehicle Model
        /// Vehicle Lot + 6  Vehicles
        ///</summary>

        public void Gen_Baseline_Test_Seed_Data(
            List<string> componentCodes = null
        ) { // todo add component codes
            Gen_VehicleTimelineEventTypes();
            Gen_ProductionStations("station_1", "station_2");

            if (componentCodes != null) {
                Gen_Components(componentCodes.ToArray());
            } else {
                Gen_Components("component_1", "component_2");
            }
            Gen_Model_From_Existing_Component_And_Stations();
            Gen_Plant_Bom_Lot_and_Kits();
        }

        public Bom Gen_Plant_Bom(string plantCode = null) {
            var plant = Gen_Plant(plantCode);
            var bom = Gen_Bom(plant.Code);
            return bom;
        }
        public void Gen_Plant_Bom_Lot_and_Kits(string plantCode = null) {
            var bom = Gen_Plant_Bom(plantCode);
            var plant = bom.Plant;
            var model = ctx.VehicleModels.First();
            var lot = Gen_VehicleLot(bom.Id, model.Id, kitCount: 6);
        }

        private void Gen_Model_From_Existing_Component_And_Stations() {

            var components = ctx.Components.ToList();
            var productionStations = ctx.ProductionStations.ToList();

            var component_station_mappings = components
                .Select(component => productionStations.Select(station =>
                    (component.Code, station.Code)
                )).SelectMany(t => t).ToList();

            var modelCode = "model_1";
            Gen_VehicleModel(
                modelCode: modelCode,
                component_stations_maps: component_station_mappings
              );
        }

        public Bom Gen_Bom(string plantCode = null) {
            var plant = ctx.Plants.First(t => t.Code == plantCode);

            var bom = new Bom {
                Plant = plant,
                Sequence = 1,
            };
            ctx.Boms.Add(bom);
            ctx.SaveChanges();
            return bom;
        }

        public VehicleLot Gen_VehicleLot(Guid bomId, Guid modelId, int kitCount = 6, bool auto_assign_vin = false) {
            var lotNo = Gen_LotNo("BP");
            var model = ctx.VehicleModels
                .Include(t => t.ModelComponents)
                .Include(t => t.ModelComponents)
                .FirstOrDefault(t => t.Id == modelId);

            var bom = ctx.Boms.First(t => t.Id == bomId);

            var lot = new VehicleLot {
                Bom = bom,
                LotNo = lotNo,
                PlantId = bom.Plant.Id,
                Vehicles = Enumerable.Range(1, kitCount)
                    .Select(kitSeq => VehicleForKitSeq(model, kitSeq))
                    .ToList()
            };
            ctx.VehicleLots.Add(lot);
            ctx.SaveChanges();
            return lot;

            Vehicle VehicleForKitSeq(VehicleModel model, int kitSeq) {
                var vehicle = new Vehicle {
                    Model = model,
                    KitNo = Gen_KitNo(lotNo, kitSeq),
                    VIN = auto_assign_vin ? Gen_VIN() : "",
                    VehicleComponents = model.ModelComponents.Select(mc => new VehicleComponent {
                        ComponentId = mc.ComponentId,
                        ProductionStationId = mc.ProductionStationId
                    }).ToList()
                };
                return vehicle;
            }
        }

        public Plant Gen_Plant(string plantCode = null) {
            plantCode = plantCode != null ? plantCode : Gen_PlantCode();

            var plant = new Plant {
                Code = plantCode,
                Name = $"{plantCode} name"
            };
            ctx.Plants.Add(plant);
            ctx.SaveChanges();
            return plant;
        }

        public List<ProductionStation> Gen_ProductionStations(params string[] codes) {
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

        public List<Component> Gen_Components(params string[] codes) {
            var componentCodes = codes.ToList().Where(code => !ctx.Components.Any(t => t.Code == code));

            var components = componentCodes.ToList().Select(code => new Component {
                Code = code,
                Name = $"{code} name"
            }).ToList();

            foreach (var component in components) {
                if (ctx.Components.Count(t => t.Code == component.Code) == 0) {
                    ctx.Components.AddRange(components);
                }
            }

            ctx.SaveChanges();
            return ctx.Components.ToList();
        }

        public VehicleModel Gen_VehicleModel(
            string modelCode,
            List<(string componentCode,
            string stationCode)> component_stations_maps
        ) {
            Gen_Components(component_stations_maps.Select(t => t.componentCode).ToArray());
            Gen_ProductionStations(component_stations_maps.Select(t => t.stationCode).ToArray());

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

        public ComponentSerial Gen_ComponentScan(Guid vehicleComponentId) {
            var vehicleComponent = ctx.VehicleComponents.FirstOrDefault(t => t.Id == vehicleComponentId);
            var componentScan = new ComponentSerial {
                VehicleComponentId = vehicleComponentId,
                Serial1 = Util.RandomString(EntityFieldLen.ComponentSerial),
                Serial2 = ""
            };
            ctx.ComponentSerials.Add(componentScan);
            ctx.SaveChanges();
            return componentScan;
        }

        public Vehicle Gen_Vehicle_Entity(
            string kitNo,
            string lotNo,
            string modelCode
        ) {
            var lot = ctx.VehicleLots.First(t => t.LotNo == lotNo);
            var model = ctx.VehicleModels.First(t => t.Code == modelCode);
            var vehicle = new Vehicle {
                Lot = lot,
                Model = model,
                KitNo = kitNo
            };
            return vehicle;
        }

        public Vehicle Gen_Vehicle_From_Model(
            string vin,
            string kitNo,
            string lotNo,
            string modelCode
            ) {

            // plant
            var plantCode = Gen_PlantCode();
            var plant = new Plant { Code = plantCode };
            ctx.Plants.Add(plant);

            // model
            var vehicleModel = ctx.VehicleModels
                .Include(t => t.ModelComponents)
                .FirstOrDefault(t => t.Code == modelCode);

            var vehicleComponents = vehicleModel.ModelComponents.Select(mc => new VehicleComponent {
                ComponentId = mc.ComponentId,
                ProductionStationId = mc.ProductionStationId
            }).ToList();

            var vehicleLot = new VehicleLot { LotNo = lotNo, Plant = plant };
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
            List<(string componentCode, string stationCode)> component_stations_maps,
            bool auto_assign_vin = false
        ) {

            // ensure component codes
            component_stations_maps.Select(t => t.componentCode).Distinct().ToList().ForEach(code => {
                if (!ctx.Components.Any(t => t.Code == code)) {
                    ctx.Components.Add(new Component{
                        Code = code,
                        Name = code + " name"
                    });
                    ctx.SaveChanges();
                }
            });
            // ensure production stations
            component_stations_maps.Select(t => t.stationCode).Distinct().ToList().ForEach(code => {
                if (!ctx.Components.Any(t => t.Code == code)) {
                    var lastSorderOrder = ctx.ProductionStations.OrderByDescending(t => t.SortOrder)
                        .Select(t => t.SortOrder)
                        .FirstOrDefault();

                    ctx.ProductionStations.Add(new ProductionStation {
                        Code = code,
                        Name = code + " name",
                        SortOrder = lastSorderOrder + 1
                    });
                    ctx.SaveChanges();
                }
            });

            var modelCode = Gen_VehicleModel_Code();
            var model = Gen_VehicleModel(
                modelCode: modelCode,
                component_stations_maps: component_stations_maps
              );

            // cretre vehicle based on that model
            var bom = ctx.Boms.Include(t => t.Plant).First();
            var plant = bom.Plant;
            var lot = Gen_VehicleLot(bom.Id, model.Id, auto_assign_vin: auto_assign_vin);

            var vehicle = ctx.Vehicles
                .Include(t => t.Lot)
                .First(t => t.Lot.Id == lot.Id);
            return vehicle;
        }

        public VehicleLotInput Gen_VehicleLotInput(
            string lotNo,
            string plantCode,
            string modelCode,
            List<string> kitNos) {
            return new VehicleLotInput {
                LotNo = lotNo,
                PlantCode = plantCode,
                Kits = kitNos.Select(kitNo => new VehicleLotInput.Kit {
                    KitNo = kitNo,
                    ModelCode = modelCode,
                }).ToList()
            };
        }

        public void SetEntityCreatedAt<T>(Guid id, DateTime date) where T : EntityBase {
            var entity = ctx.Find<T>(id);
            entity.CreatedAt = date;
            ctx.SaveChanges();
        }

        public void Gen_VehicleTimelineEventTypes() {
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

            foreach (var eventType in eventTypes) {
                if (ctx.VehicleTimelineEventTypes.Count(t => t.Code == eventType.Code) == 0) {
                    ctx.VehicleTimelineEventTypes.AddRange(eventTypes);
                }
            }
            ctx.SaveChanges();
        }

        #region generators for specific entity fields
        public string Get_Code(int len) {
            return Util.RandomString(len).ToUpper();
        }
        public string Gen_LotNo(string prefix = "BP") {
            return prefix + Util.RandomString(EntityFieldLen.Vehicle_LotNo - prefix.Length).ToUpper();
        }
        public string Gen_KitNo(string prefix = "", int kitSequence = 1) {
            var suffix = kitSequence.ToString().PadLeft(2, '0');
            return
                prefix +
                Util.RandomString(EntityFieldLen.Vehicle_KitNo - (prefix.Length + suffix.Length)).ToUpper() +
                suffix;
        }
        public string Gen_VehicleModel_Code() {
            return Util.RandomString(EntityFieldLen.VehicleModel_Code).ToUpper();
        }
        public string Gen_VIN() {
            return Util.RandomString(EntityFieldLen.Vehicle_VIN).ToUpper();
        }
        public string Gen_ComponentCode() {
            return Util.RandomString(EntityFieldLen.Component_Code).ToUpper();
        }
        public string Gen_ProductionStationCode() {
            return Util.RandomString(EntityFieldLen.ProductionStation_Code).ToUpper();
        }
        public string Gen_PlantCode() {
            return Util.RandomString(EntityFieldLen.Plant_Code).ToUpper();
        }
        public string Get_PlantCode() {
            return Util.RandomString(EntityFieldLen.Plant_Code).ToUpper();
        }
        public string Gen_PartNo() {
            return Util.RandomString(EntityFieldLen.Part_No).ToUpper();
        }
        public string Gen_PartDesc() {
            return Util.RandomString(EntityFieldLen.Part_Desc).ToUpper();
        }
        public string Gen_ShipmentInvoiceNo() {
            return Util.RandomString(EntityFieldLen.Shipment_InvoiceNo).ToUpper();
        }
        public string Gen_ComponentSerialNo() {
            return Util.RandomString(EntityFieldLen.ComponentSerial).ToUpper();
        }

        #endregion
    }
}