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
            bool generateLot = true,
            List<string> componentCodes = null
        ) { // todo add component codes
            Gen_KitTimelineEventTypes();
            Gen_ProductionStations("station_1", "station_2");

            if (componentCodes != null) {
                Gen_Components(componentCodes.ToArray());
            } else {
                Gen_Components("component_1", "component_2");
            }
            Gen_Model_From_Existing_Component_And_Stations();
            var bom = Gen_Plant_Bom();
            if (generateLot) {
                var plant = bom.Plant;
                var model = ctx.VehicleModels.First();
                var lot = Gen_Lot(bom.Id, model.Id, kitCount: 6);
            }
        }
        public Bom Gen_Plant_Bom(string plantCode = null) {
            var plant = Gen_Plant(plantCode);
            var bom = Gen_Bom(plant.Code);
            return bom;
        }
        public void Gen_Bom_Lot_and_Kits(string plantCode = null) {
            var bom = Gen_Plant_Bom(plantCode);
            var plant = bom.Plant;
            var model = ctx.VehicleModels.First();
            var lot = Gen_Lot(bom.Id, model.Id, kitCount: 6);
        }

        public void Gen_Model_From_Existing_Component_And_Stations() {

            var components = ctx.Components.ToList();
            var productionStations = ctx.ProductionStations.ToList();

            var component_station_mappings = components
                .Select(component => productionStations.Select(station =>
                    (component.Code, station.Code)
                )).SelectMany(t => t).ToList();

            var modelCode = Gen_VehicleModel_Code();
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

        public Lot Gen_Lot(Guid bomId, Guid modelId, int kitCount = 6, bool auto_assign_vin = false) {
            var model = ctx.VehicleModels
                .Include(t => t.ModelComponents)
                .Include(t => t.ModelComponents)
                .FirstOrDefault(t => t.Id == modelId);

            var lotNo = Gen_NewLotNo(model.Code);

            var bom = ctx.Boms.First(t => t.Id == bomId);

            var lot = new Lot {
                Bom = bom,
                LotNo = lotNo,
                Model = model,
                PlantId = bom.Plant.Id,
                Kits = Enumerable.Range(1, kitCount)
                    .Select(kitSeq => VehicleForKitSeq(model, kitSeq))
                    .ToList()
            };
            ctx.Lots.Add(lot);
            ctx.SaveChanges();
            return lot;

            Kit VehicleForKitSeq(VehicleModel model, int kitSeq) {
                var vehicle = new Kit {
                    KitNo = Gen_KitNo(lotNo, kitSeq),
                    VIN = auto_assign_vin ? Gen_VIN() : "",
                    KitComponents = model.ModelComponents.Select(mc => new KitComponent {
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
                Sequence = index + 1
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
                KitComponentId = vehicleComponentId,
                Serial1 = Util.RandomString(EntityFieldLen.ComponentSerial),
                Serial2 = ""
            };
            ctx.ComponentSerials.Add(componentScan);
            ctx.SaveChanges();
            return componentScan;
        }

        public Kit Gen_Kit_From_Model(
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

            var vehicleComponents = vehicleModel.ModelComponents.Select(mc => new KitComponent {
                ComponentId = mc.ComponentId,
                ProductionStationId = mc.ProductionStationId
            }).ToList();

            var vehicleLot = new Lot { LotNo = lotNo, Plant = plant };
            ctx.Lots.Add(vehicleLot);

            var vehicle = new Kit {
                VIN = vin,
                Lot = vehicleLot,
                KitNo = kitNo,
                KitComponents = vehicleComponents
            };

            ctx.Kits.AddRange(vehicle);
            ctx.SaveChanges();

            return vehicle;
        }

        public Kit Gen_Kit_Amd_Model_From_Components(
            List<(string componentCode, string stationCode)> component_stations_maps,
            bool auto_assign_vin = false
        ) {

            // ensure component codes
            component_stations_maps.Select(t => t.componentCode).Distinct().ToList().ForEach(code => {
                if (!ctx.Components.Any(t => t.Code == code)) {
                    ctx.Components.Add(new Component {
                        Code = code,
                        Name = code + " name"
                    });
                    ctx.SaveChanges();
                }
            });
            // ensure production stations
            component_stations_maps.Select(t => t.stationCode).Distinct().ToList().ForEach(code => {
                if (!ctx.Components.Any(t => t.Code == code)) {
                    var lastSorderOrder = ctx.ProductionStations.OrderByDescending(t => t.Sequence)
                        .Select(t => t.Sequence)
                        .FirstOrDefault();

                    ctx.ProductionStations.Add(new ProductionStation {
                        Code = code,
                        Name = code + " name",
                        Sequence = lastSorderOrder + 1
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
            var lot = Gen_Lot(bom.Id, model.Id, auto_assign_vin: auto_assign_vin);

            var vehicle = ctx.Kits
                .Include(t => t.Lot)
                .First(t => t.Lot.Id == lot.Id);
            return vehicle;
        }

        public void SetEntityCreatedAt<T>(Guid id, DateTime date) where T : EntityBase {
            var entity = ctx.Find<T>(id);
            entity.CreatedAt = date;
            ctx.SaveChanges();
        }

        public void Gen_KitTimelineEventTypes() {
            var eventTypes = new List<KitTimelineEventType> {
                new KitTimelineEventType {
                    Code = TimeLineEventType.CUSTOM_RECEIVED.ToString(),
                },
                new KitTimelineEventType {
                    Code = TimeLineEventType.PLAN_BUILD.ToString(),
                },
                new KitTimelineEventType {
                    Code = TimeLineEventType.BULD_COMPLETED.ToString(),
                },
                new KitTimelineEventType {
                    Code = TimeLineEventType.GATE_RELEASED.ToString(),
                },
                new KitTimelineEventType {
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
        public string Gen_LotNo(string modelCode, int sequence) {
            return modelCode + sequence.ToString().PadLeft(EntityFieldLen.LotNo - EntityFieldLen.VehicleModel_CodeLen, '0');
        }


        public string Gen_LotNo(int sequence) {
            var modelCode = ctx.VehicleModels.Select(t => t.Code).First();
            return modelCode + sequence.ToString().PadLeft(EntityFieldLen.LotNo - EntityFieldLen.VehicleModel_CodeLen, '0');
        }

        public string Gen_NewLotNo(string modelCode) {
            var sequence = 1;
            var lotNo = modelCode + sequence.ToString().PadLeft(EntityFieldLen.LotNo - EntityFieldLen.VehicleModel_CodeLen, '0');
            var lotExists = ctx.Lots.Any(t => t.LotNo == lotNo);
            while(lotExists) {
                sequence++;
                lotNo = modelCode + sequence.ToString().PadLeft(EntityFieldLen.LotNo - EntityFieldLen.VehicleModel_CodeLen, '0');
                lotExists = ctx.Lots.Any(t => t.LotNo == lotNo);
            }
            return lotNo;
        }


        public string Gen_KitNo(string prefix = "", int kitSequence = 1) {
            var suffix = kitSequence.ToString().PadLeft(2, '0');
            return
                prefix +
                Util.RandomString(EntityFieldLen.Vehicle_KitNo - (prefix.Length + suffix.Length)).ToUpper() +
                suffix;
        }
        public string Gen_VehicleModel_Code() {
            return Util.RandomString(EntityFieldLen.VehicleModel_CodeLen).ToUpper();
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