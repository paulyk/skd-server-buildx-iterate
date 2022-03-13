
namespace SKD.Test;
using PartQuantities = IEnumerable<(string partNo, int quantity)>;

public class TestBase {

    protected SkdContext context;
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
        bool assignVin = false,
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
            var model = context.VehicleModels.First();
            Gen_Lot(bom.Id, model.Id, kitCount: 6, assignVin: assignVin);
        }
        Gen_Dealers();
    }
    public Bom Gen_Plant_Bom(string plantCode = null) {
        var plant = Gen_Plant(plantCode);
        var bom = Gen_Bom(plant.Code);
        return bom;
    }

    public Dealer Gen_Dealers() {
        var dealer = new Dealer {
            Code = Util.RandomString(10),
            Name = Util.RandomString(10)
        };
        context.Dealers.Add(dealer);
        context.SaveChanges();
        return dealer;
    }
    public void Gen_Bom_Lot_and_Kits(string plantCode = null, bool assignVin = false) {
        var bom = Gen_Plant_Bom(plantCode);
        var model = context.VehicleModels.First();
        Gen_Lot(bom.Id, model.Id, kitCount: 6, assignVin: assignVin);
    }

    public void Gen_Model_From_Existing_Component_And_Stations() {

        var components = context.Components.ToList();
        var productionStations = context.ProductionStations.ToList();

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
        var plant = context.Plants.First(t => t.Code == plantCode);

        var bom = new Bom {
            Plant = plant,
            Sequence = 1,
        };
        context.Boms.Add(bom);
        context.SaveChanges();
        return bom;
    }

    public Lot Gen_Lot(Guid bomId, Guid modelId, int kitCount = 6, bool assignVin = false) {
        var model = context.VehicleModels
            .Include(t => t.ModelComponents)
            .Include(t => t.ModelComponents)
            .FirstOrDefault(t => t.Id == modelId);

        var lotNo = Gen_NewLotNo(model.Code);

        var bom = context.Boms.First(t => t.Id == bomId);

        var lot = new Lot {
            Bom = bom,
            LotNo = lotNo,
            Model = model,
            PlantId = bom.Plant.Id,
            Kits = Enumerable.Range(1, kitCount)
                .Select(kitSeq => VehicleForKitSeq(model, kitSeq))
                .ToList()
        };
        context.Lots.Add(lot);
        context.SaveChanges();
        return lot;

        Kit VehicleForKitSeq(VehicleModel model, int kitSeq) {
            var vehicle = new Kit {
                KitNo = Gen_KitNo(lotNo, kitSeq),
                VIN = assignVin ? Gen_VIN() : "",
                KitComponents = model.ModelComponents.Select(mc => new KitComponent {
                    ComponentId = mc.ComponentId,
                    ProductionStationId = mc.ProductionStationId
                }).ToList()
            };
            return vehicle;
        }
    }

    public Plant Gen_Plant(string plantCode = null) {
        plantCode = plantCode ?? Gen_PlantCode();

        var plant = new Plant {
            Code = plantCode,
            PartnerPlantCode = Gen_PartnerPLantCode(),
            PartnerPlantType = Gen_PartnerPlantType(),
            Name = $"{plantCode} name"
        };
        context.Plants.Add(plant);
        context.SaveChanges();
        return plant;
    }

    public async Task Gen_KitVinImport(string kitNo, string vin) {

        var kit = await context.Kits
            .Include(t => t.Lot).ThenInclude(t => t.Plant)
            .Include(t => t.KitVins)
            .FirstAsync(t => t.KitNo == kitNo);

        context.KitVinImports.Add(new KitVinImport {
            Plant = kit.Lot.Plant,
            PartnerPlantCode = kit.Lot.Plant.PartnerPlantCode,
            Sequence = 1,
            KitVins = new List<KitVin> {
                new KitVin {
                    Kit = kit,
                    VIN = vin
                }
            }
        });

        kit.VIN = vin;

        await context.SaveChangesAsync();

        kit = await context.Kits
            .Include(t => t.KitVins)
            .FirstAsync(t => t.KitNo == kitNo);
    }

    public async Task Gen_KitComponentSerial(string kitNo, string componentCode, string serial1, string serial2, bool verify) {
        var kitComponent = await context.KitComponents.FirstOrDefaultAsync(t => t.Kit.KitNo == kitNo && t.Component.Code == componentCode);
        var componentSerial = new ComponentSerial {
            Serial1 = serial1,
            Serial2 = serial2,
            VerifiedAt = verify ? DateTime.Now : (DateTime?)null,
        };
        kitComponent.ComponentSerials.Add(componentSerial);
        await context.SaveChangesAsync();
    }

    public List<ProductionStation> Gen_ProductionStations(params string[] codes) {
        var stationCodes = codes.Where(code => !context.ProductionStations.Any(t => t.Code == code)).ToList();

        var productionStations = stationCodes.ToList().Select((code, index) => new ProductionStation {
            Code = code,
            Name = $"{code} name",
            Sequence = index + 1
        });


        context.ProductionStations.AddRange(productionStations);
        context.SaveChanges();
        return context.ProductionStations.ToList();
    }

    public List<Component> Gen_Components(params string[] codes) {
        var componentCodes = codes.ToList().Where(code => !context.Components.Any(t => t.Code == code));

        var components = componentCodes.ToList().Select(code => new Component {
            Code = code,
            Name = $"{code} name",
            ComponentSerialRule = ComponentSerialRule.ONE_OR_BOTH_SERIALS
        }).ToList();

        foreach (var component in components) {
            if (!context.Components.Any(t => t.Code == component.Code)) {
                context.Components.AddRange(component);
            }
        }

        context.SaveChanges();
        return context.Components.ToList();
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
            Component = context.Components.First(t => t.Code == map.componentCode),
            ProductionStation = context.ProductionStations.First(t => t.Code == map.stationCode)
        }).ToList();

        var vehicleModel = new VehicleModel {
            Code = modelCode,
            Description = $"{modelCode} name",
            ModelComponents = modelComponents
        };

        context.VehicleModels.Add(vehicleModel);
        context.SaveChanges();
        return vehicleModel;
    }

    public ComponentSerial Gen_ComponentScan(Guid vehicleComponentId) {
        var vehicleComponent = context.KitComponents.FirstOrDefault(t => t.Id == vehicleComponentId);
        var componentScan = new ComponentSerial {
            KitComponentId = vehicleComponentId,
            Serial1 = Util.RandomString(EntityFieldLen.ComponentSerial),
            Serial2 = ""
        };
        context.ComponentSerials.Add(componentScan);
        context.SaveChanges();
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
        context.Plants.Add(plant);

        // model
        var vehicleModel = context.VehicleModels
            .Include(t => t.ModelComponents)
            .FirstOrDefault(t => t.Code == modelCode);

        var vehicleComponents = vehicleModel.ModelComponents.Select(mc => new KitComponent {
            ComponentId = mc.ComponentId,
            ProductionStationId = mc.ProductionStationId
        }).ToList();

        var vehicleLot = new Lot { LotNo = lotNo, Plant = plant };
        context.Lots.Add(vehicleLot);

        var vehicle = new Kit {
            VIN = vin,
            Lot = vehicleLot,
            KitNo = kitNo,
            KitComponents = vehicleComponents
        };

        context.Kits.AddRange(vehicle);
        context.SaveChanges();

        return vehicle;
    }

    public Kit Gen_Kit_Amd_Model_From_Components(
        List<(string componentCode, string stationCode)> component_stations_maps,
        bool auto_assign_vin = false
    ) {

        // ensure component codes
        component_stations_maps.Select(t => t.componentCode).Distinct().ToList().ForEach(code => {
            if (!context.Components.Any(t => t.Code == code)) {
                context.Components.Add(new Component {
                    Code = code,
                    Name = code + " name",
                    ComponentSerialRule = ComponentSerialRule.ONE_OR_BOTH_SERIALS
                });
                context.SaveChanges();
            }
        });
        // ensure production stations
        component_stations_maps.Select(t => t.stationCode).Distinct().ToList().ForEach(code => {
            if (!context.Components.Any(t => t.Code == code)) {
                var lastSorderOrder = context.ProductionStations.OrderByDescending(t => t.Sequence)
                    .Select(t => t.Sequence)
                    .FirstOrDefault();

                context.ProductionStations.Add(new ProductionStation {
                    Code = code,
                    Name = code + " name",
                    Sequence = lastSorderOrder + 1
                });
                context.SaveChanges();
            }
        });

        var modelCode = Gen_VehicleModel_Code();
        var model = Gen_VehicleModel(
            modelCode: modelCode,
            component_stations_maps: component_stations_maps
          );

        // cretre vehicle based on that model
        var bom = context.Boms.Include(t => t.Plant).First();
        var plant = bom.Plant;
        var lot = Gen_Lot(bom.Id, model.Id, assignVin: auto_assign_vin);

        var kit = context.Kits
            .Include(t => t.Lot)
            .First(t => t.Lot.Id == lot.Id);
        return kit;
    }

    public void SetEntityCreatedAt<T>(Guid id, DateTime date) where T : EntityBase {
        var entity = context.Find<T>(id);
        entity.CreatedAt = date;
        context.SaveChanges();
    }

    public void Gen_KitTimelineEventTypes() {

        var eventTypes = Enum.GetValues<TimeLineEventCode>()
            .Select((code, i) => new KitTimelineEventType() {
                Code = code,
                Description = code.ToString(),
                Sequence = i + 1
            }).ToList();

        // var sequence = 1;
        // eventTypes.ForEach(eventType => {
        //     eventType.Description = eventType.Code.ToString();
        //     eventType.Sequence = sequence++;
        // });

        foreach (var eventType in eventTypes) {
            if (!context.KitTimelineEventTypes.Any(t => t.Code == eventType.Code)) {
                context.KitTimelineEventTypes.AddRange(eventTypes);
            }
        }
        context.SaveChanges();
    }


    public async Task Gen_ShipmentLot_ForKit(string kitNo) {
        var kit = await context.Kits
            .Include(t => t.Lot)
            .FirstOrDefaultAsync(t => t.KitNo == kitNo);

        await Gen_ShipmentLot(kit.Lot.LotNo);
    }

    public async Task Gen_ShipmentLot(string lotNo) {
        var lot = await context.Lots
            .Include(t => t.Bom).ThenInclude(t => t.Plant)
            .FirstOrDefaultAsync(t => t.LotNo == lotNo);
            
        if (await context.ShipmentLots.AnyAsync(t => t.Lot.LotNo == lot.LotNo)) {
            return;
        }

        var shipment = new Shipment {
            Plant = lot.Bom.Plant,
            Sequence = 2,
            ShipmentLots = new List<ShipmentLot> {
                    new ShipmentLot {
                        Lot = lot
                    }
                }
        };

        context.Shipments.Add(shipment);
        await context.SaveChangesAsync();
    }

    #region generators for specific entity fields
    public string Get_Code(int len) {
        return Util.RandomString(len).ToUpper();
    }
    public string Gen_LotNo(string modelCode, int sequence) {
        return modelCode + sequence.ToString().PadLeft(EntityFieldLen.LotNo - EntityFieldLen.VehicleModel_Code, '0');
    }

    public string Gen_PartnerPLantCode() {
        return Util.RandomString(EntityFieldLen.PartnerPlant_Code);
    }

    public string Gen_LotNo(int sequence) {
        var modelCode = context.VehicleModels.Select(t => t.Code).First();
        return modelCode + sequence.ToString().PadLeft(EntityFieldLen.LotNo - EntityFieldLen.VehicleModel_Code, '0');
    }

    public string Gen_NewLotNo(string modelCode) {
        var sequence = 1;
        var lotNo = modelCode + sequence.ToString().PadLeft(EntityFieldLen.LotNo - EntityFieldLen.VehicleModel_Code, '0');
        var lotExists = context.Lots.Any(t => t.LotNo == lotNo);
        while (lotExists) {
            sequence++;
            lotNo = modelCode + sequence.ToString().PadLeft(EntityFieldLen.LotNo - EntityFieldLen.VehicleModel_Code, '0');
            lotExists = context.Lots.Any(t => t.LotNo == lotNo);
        }
        return lotNo;
    }


    public string Gen_KitNo(string prefix = "", int kitSequence = 1) {
        var suffix = kitSequence.ToString().PadLeft(2, '0');
        return
            prefix +
            Util.RandomString(EntityFieldLen.KitNo - (prefix.Length + suffix.Length)).ToUpper() +
            suffix;
    }
    public string Gen_VehicleModel_Code() {
        return Util.RandomString(EntityFieldLen.VehicleModel_Code).ToUpper();
    }

    public string Gen_VehicleModel_Description() {
        return Util.RandomString(EntityFieldLen.VehicleModel_Description).ToUpper();
    }
    public string Gen_VehilceModel_Meta() {
        return Util.RandomString(EntityFieldLen.VehicleModel_Meta).ToUpper();
    }
    public string Gen_VIN() {
        return Util.RandomString(EntityFieldLen.VIN).ToUpper();
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
    public string Gen_PartnerPlantCode() {
        return Util.RandomString(EntityFieldLen.PartnerPlant_Code).ToUpper();
    }

    public string Gen_PartnerPlantType() {
        return Util.RandomString(EntityFieldLen.PartnerPlant_Type).ToUpper();
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

    #region bom import 

    public BomFile Gen_BomFileInput(string plantCode, IEnumerable<string> lotNumbers, int kitCount, PartQuantities partQuantities) {
        return new BomFile() {
            PlantCode = plantCode,
            Sequence = 1,
            LotEntries = Gen_LotEntries(lotNumbers, kitCount),
            LotParts = Gen_BomLotParts(lotNumbers, partQuantities)
        };
    }

    private List<BomFile.BomFileLot> Gen_LotEntries(IEnumerable<string> lotNumbers, int kitCount) {
        return lotNumbers.Select(lotNo => new BomFile.BomFileLot {
            LotNo = lotNo,
            Kits = Enumerable.Range(1, kitCount).Select(num => new BomFile.BomFileLot.BomFileKit {
                KitNo = Gen_KitNo(lotNo, num),
                ModelCode = lotNo.Substring(0, EntityFieldLen.VehicleModel_Code)
            }).ToList()
        }).ToList();
    }

    private List<BomFile.BomFileLotPart> Gen_BomLotParts(IEnumerable<string> lotNumbers, PartQuantities partQuantities) {
        if (!partQuantities.Any()) {
            return new List<BomFile.BomFileLotPart>();
        }

        return lotNumbers.SelectMany(t =>
            partQuantities.Select(lp => new BomFile.BomFileLotPart {
                LotNo = t,
                PartNo = lp.partNo,
                PartDesc = lp.partNo + " desc",
                Quantity = lp.quantity
            })
        ).ToList();
    }
    #endregion
}
