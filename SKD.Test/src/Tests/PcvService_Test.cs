namespace SKD.Test;

public class PcvServiceTest : TestBase {

    public PcvServiceTest() {
        context = GetAppDbContext();
    }

    [Fact]
    public async Task Can_add_pcv() {
        // setup
        var input = GenPcvInput();
        var service = new PcvService(context);

        // test
        var model_before_count = await context.Pcvs.CountAsync();
        var component_before_count = await context.PcvComponents.CountAsync();

        var result = await service.Save(input);

        // assert
        var model_after_count = await context.Pcvs.CountAsync();
        Assert.Equal(model_before_count + 1, model_after_count);

        var pcv = await context.Pcvs.FirstOrDefaultAsync(t => t.Code == input.Code);

        Assert.Equal(input.Description, pcv.Description);
        Assert.Equal(input.Model, pcv.Model);
        Assert.Equal(input.ModelYear, pcv.ModelYear);
        Assert.Equal(input.Series, pcv.Series);
        Assert.Equal(input.Body, pcv.Body);
    }

    [Fact]
    public async Task Cannot_save_if_duplicate_code_or_name() {
        // setup
        var input = GenPcvInput();

        var service = new PcvService(context);

        // test        
        await service.Save(input);
        var model_count_1 = await context.Pcvs.CountAsync();
        var model_component_count_1 = await context.PcvComponents.CountAsync();

        var result_1 = await service.Save(input);
        var errors = result_1.Errors.Select(t => t.Message).ToList();

        var ducplicateCode = errors.Any(error => error.StartsWith("duplicate code"));
        Assert.True(ducplicateCode);
    }
    [Fact]
    public async Task Can_modify_pcv_name() {
        // setup
        var input = GenPcvInput();
        var service = new PcvService(context);

        // test        
        await service.Save(input);

        var pcv = await context.Pcvs
            .Include(t => t.PcvComponents).ThenInclude(t => t.Component)
            .Include(t => t.PcvComponents).ThenInclude(t => t.ProductionStation)
        .FirstOrDefaultAsync(t => t.Code == input.Code);

        Assert.Equal(input.Description, pcv.Description);

        // modify name
        var input_2 = new PcvInput {
            Id = pcv.Id,
            Code = pcv.Code,
            Description = Gen_Pcv_Description(),
            ComponentStationInputs = pcv.PcvComponents.Select(t => new ComponentStationInput {
                ComponentCode = t.Component.Code,
                ProductionStationCode = t.ProductionStation.Code
            }).ToList()
        };
        await service.Save(input_2);

        pcv = await context.Pcvs.FirstOrDefaultAsync(t => t.Code == input.Code);

        Assert.Equal(input_2.Description, pcv.Description);
    }

    [Fact]
    public async Task Cannot_create_pcv_without_components() {
        // setup
        var service = new PcvService(context);
        var before_count = await context.Pcvs.CountAsync();

        var model_1 = new PcvInput {
            Code = Util.RandomString(EntityFieldLen.Pcv_Code),
            Description = Util.RandomString(EntityFieldLen.Pcv_Description)
        };
        var result = await service.Save(model_1);

        //test
        var after_count = await context.Pcvs.CountAsync();
        Assert.Equal(before_count, after_count);

        var errorCount = result.Errors.Count;
        Assert.Equal(1, errorCount);
    }

    [Fact]
    public async Task Cannot_add_pcv_with_duplicate_component_station_entries() {
        // setup
        Gen_Components("component_1", "component_2");
        Gen_ProductionStations("station_1", "station_2");

        var component = context.Components.OrderBy(t => t.Code).First();
        var station = context.ProductionStations.OrderBy(t => t.Code).First();

        var vehilceModel = new PcvInput {
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
        var service = new PcvService(context);
        var result = await service.Save(vehilceModel);

        // assert
        var errorCount = result.Errors.Count;
        Assert.Equal(1, errorCount);
        var expectedErrorMessage = "duplicate component + production station entries";
        var actualErrorMessage = result.Errors.Select(t => t.Message).FirstOrDefault();
        Assert.StartsWith(expectedErrorMessage, actualErrorMessage);
    }

    [Fact]
    public async Task Can_create_pcv_from_existing() {
        // setup          
        var templateModelInput = GenPcvInput();
        var service = new PcvService(context);
        await service.Save(templateModelInput);
        var existingModel = await context.Pcvs.FirstOrDefaultAsync(t => t.Code == templateModelInput.Code);

        // test
        var newModelInput = new PcvFromExistingInput {
            Code = Gen_Pcv_Code(),
            ModelYear = "2030",
            ExistingModelCode = existingModel.Code
        };

        var result = await service.CreateFromExisting(newModelInput);

        // assert
        var newModel = await context.Pcvs.FirstOrDefaultAsync(t => t.Code == newModelInput.Code);

        Assert.Equal(existingModel.Description, newModel.Description);

        var templateModelComponents = await context.PcvComponents
                .OrderBy(t => t.ProductionStation.Code).ThenBy(t => t.Component.Code)
                .Where(t => t.Pcv.Code == templateModelInput.Code)
                .ToListAsync();

        var newModelComponents = await context.PcvComponents
                .OrderBy(t => t.ProductionStation.Code).ThenBy(t => t.Component.Code)
                .Where(t => t.Pcv.Code == newModelInput.Code)
                .ToListAsync();


        for (var i = 0; i < templateModelComponents.Count; i++) {
            var templateEntry = templateModelComponents[i];
            var newEntry = newModelComponents[i];

            Assert.Equal(templateEntry.ComponentId, newEntry.ComponentId);
            Assert.Equal(templateEntry.ProductionStationId, newEntry.ProductionStationId);
        }
    }




    private PcvInput GenPcvInput() {
        var componentCodes = new string[] { "component_1", "component_2" };
        var stationCodes = new string[] { "station_1", "station_2" };
        Gen_Components(componentCodes);
        Gen_ProductionStations(stationCodes);

        return new PcvInput {
            Code = Gen_Pcv_Code(),
            Description = Gen_Pcv_Description(),
            ModelYear = DateTime.Now.Year.ToString(),
            Model = Gen_Pcv_Meta(),
            Series = Gen_Pcv_Meta(),
            Body = Gen_Pcv_Meta(),
            ComponentStationInputs = Enumerable.Range(0, componentCodes.Length)
                .Select(i => new ComponentStationInput {
                    ComponentCode = componentCodes[i],
                    ProductionStationCode = stationCodes[i]
                }).ToList()
        };
    }
}

