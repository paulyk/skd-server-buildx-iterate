namespace SKD.Test;

public class VehicleModelServiceTest : TestBase {

    public VehicleModelServiceTest() {
        context = GetAppDbContext();
    }

    [Fact]
    public async Task Can_add_vehicle_model() {
        // setup
        var input = GenVehilceModelInput();
        var service = new VehicleModelService(context);

        // test
        var model_before_count = await context.VehicleModels.CountAsync();
        var component_before_count = await context.VehicleModelComponents.CountAsync();

        var result = await service.Save(input);

        // assert
        var model_after_count = await context.VehicleModels.CountAsync();
        Assert.Equal(model_before_count + 1, model_after_count);

        var vehicleModel = await context.VehicleModels.FirstOrDefaultAsync(t => t.Code == input.Code);

        Assert.Equal(input.Description, vehicleModel.Description);
        Assert.Equal(input.Model, vehicleModel.Model);
        Assert.Equal(input.ModelYear, vehicleModel.ModelYear);
        Assert.Equal(input.Series, vehicleModel.Series);
        Assert.Equal(input.Body, vehicleModel.Body);
    }

    [Fact]
    public async Task Cannot_save_if_duplicate_code_or_name() {
        // setup
        var input = GenVehilceModelInput();

        var service = new VehicleModelService(context);

        // test        
        await service.Save(input);
        var model_count_1 = await context.VehicleModels.CountAsync();
        var model_component_count_1 = await context.VehicleModelComponents.CountAsync();

        var result_1 = await service.Save(input);
        var errors = result_1.Errors.Select(t => t.Message).ToList();

        var ducplicateCode = errors.Any(error => error.StartsWith("duplicate code"));
        Assert.True(ducplicateCode);
    }
    [Fact]
    public async Task Can_modify_model_name() {
        // setup
        var input = GenVehilceModelInput();
        var service = new VehicleModelService(context);

        // test        
        await service.Save(input);

        var model = await context.VehicleModels
            .Include(t => t.ModelComponents).ThenInclude(t => t.Component)
            .Include(t => t.ModelComponents).ThenInclude(t => t.ProductionStation)
        .FirstOrDefaultAsync(t => t.Code == input.Code);

        Assert.Equal(input.Description, model.Description);

        // modify name
        var input_2 = new VehicleModelInput {
            Id = model.Id,
            Code = model.Code,
            Description = Gen_VehicleModel_Description(),
            ComponentStationInputs = model.ModelComponents.Select(t => new ComponentStationInput {
                ComponentCode = t.Component.Code,
                ProductionStationCode = t.ProductionStation.Code
            }).ToList()
        };
        await service.Save(input_2);

        model = await context.VehicleModels.FirstOrDefaultAsync(t => t.Code == input.Code);

        Assert.Equal(input_2.Description, model.Description);
    }

    [Fact]
    public async Task Cannot_create_vehicle_model_without_components() {
        // setup
        var service = new VehicleModelService(context);
        var before_count = await context.VehicleModels.CountAsync();

        var model_1 = new VehicleModelInput {
            Code = Util.RandomString(EntityFieldLen.VehicleModel_Code),
            Description = Util.RandomString(EntityFieldLen.VehicleModel_Description)
        };
        var result = await service.Save(model_1);

        //test
        var after_count = await context.VehicleModels.CountAsync();
        Assert.Equal(before_count, after_count);

        var errorCount = result.Errors.Count;
        Assert.Equal(1, errorCount);
    }

    [Fact]
    public async Task Cannot_add_vehicle_model_with_duplicate_component_station_entries() {
        // setup
        Gen_Components("component_1", "component_2");
        Gen_ProductionStations("station_1", "station_2");

        var component = context.Components.OrderBy(t => t.Code).First();
        var station = context.ProductionStations.OrderBy(t => t.Code).First();

        var vehilceModel = new VehicleModelInput {
            Code = "Model_1",
            Description = "Model Name",
            ComponentStationInputs = new List<ComponentStationInput> {
                    new ComponentStationInput {
                        ComponentCode = component.Code,
                        ProductionStationCode = station.Code
                    },
                    new ComponentStationInput {
                        ComponentCode = component.Code,
                        ProductionStationCode = station.Code
                    },
                }
        };

        // test
        var service = new VehicleModelService(context);
        var result = await service.Save(vehilceModel);

        // assert
        var errorCount = result.Errors.Count;
        Assert.Equal(1, errorCount);
        var expectedErrorMessage = "duplicate component + production station entries";
        var actualErrorMessage = result.Errors.Select(t => t.Message).FirstOrDefault();
        Assert.StartsWith(expectedErrorMessage, actualErrorMessage);
    }

    [Fact]
    public async Task Can_create_vehicle_model_from_existing() {
        // setup          
        var templateModelInput = GenVehilceModelInput();
        var service = new VehicleModelService(context);
        await service.Save(templateModelInput);
        var existingModel = await context.VehicleModels.FirstOrDefaultAsync(t => t.Code == templateModelInput.Code);

        // test
        var newModelInput = new VehicleModelFromExistingInput {
            Code = Gen_VehicleModel_Code(),
            ModelYear = "2030",
            ExistingModelCode = existingModel.Code
        };

        var result = await service.CreateFromExisting(newModelInput);

        // assert
        var newModel = await context.VehicleModels.FirstOrDefaultAsync(t => t.Code == newModelInput.Code);

        Assert.Equal(existingModel.Description, newModel.Description);

        var templateModelComponents = await context.VehicleModelComponents
                .OrderBy(t => t.ProductionStation.Code).ThenBy(t => t.Component.Code)
                .Where(t => t.VehicleModel.Code == templateModelInput.Code)
                .ToListAsync();

        var newModelComponents = await context.VehicleModelComponents
                .OrderBy(t => t.ProductionStation.Code).ThenBy(t => t.Component.Code)
                .Where(t => t.VehicleModel.Code == newModelInput.Code)
                .ToListAsync();


        for (var i = 0; i < templateModelComponents.Count; i++) {
            var templateEntry = templateModelComponents[i];
            var newEntry = newModelComponents[i];

            Assert.Equal(templateEntry.ComponentId, newEntry.ComponentId);
            Assert.Equal(templateEntry.ProductionStationId, newEntry.ProductionStationId);
        }
    }




    private VehicleModelInput GenVehilceModelInput() {
        var componentCodes = new string[] { "component_1", "component_2" };
        var stationCodes = new string[] { "station_1", "station_2" };
        Gen_Components(componentCodes);
        Gen_ProductionStations(stationCodes);

        return new VehicleModelInput {
            Code = Gen_VehicleModel_Code(),
            Description = Gen_VehicleModel_Description(),
            ModelYear = DateTime.Now.Year.ToString(),
            Model = Gen_VehilceModel_Meta(),
            Series = Gen_VehilceModel_Meta(),
            Body = Gen_VehilceModel_Meta(),
            ComponentStationInputs = Enumerable.Range(0, componentCodes.Length)
                .Select(i => new ComponentStationInput {
                    ComponentCode = componentCodes[i],
                    ProductionStationCode = stationCodes[i]
                }).ToList()
        };
    }
}

