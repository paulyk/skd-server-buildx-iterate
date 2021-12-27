namespace SKD.Test;
public class ProductionStationServiceTest : TestBase {

    public ProductionStationServiceTest() {
        context = GetAppDbContext();
    }

    [Fact]
    public async Task Can_save_new_production_station() {
        var service = new ProductionStationService(context);
        var productionStationDTO = new ProductionStationInput() {
            Code = Util.RandomString(EntityFieldLen.ProductionStation_Code),
            Name = Util.RandomString(EntityFieldLen.ProductionStation_Name)
        };

        var before_count = await context.Components.CountAsync();
        var result = await service.SaveProductionStation(productionStationDTO);

        Assert.NotNull(result.Payload);
        var expectedCount = before_count + 1;
        var actualCount = context.ProductionStations.Count();
        Assert.Equal(expectedCount, actualCount);
    }

    [Fact]
    public async Task Can_update_new_production_station() {
        var service = new ProductionStationService(context);
        var productionStationDTO = new ProductionStationInput() {
            Code = Util.RandomString(EntityFieldLen.ProductionStation_Code),
            Name = Util.RandomString(EntityFieldLen.ProductionStation_Name)
        };

        var before_count = await context.Components.CountAsync();
        var result = await service.SaveProductionStation(productionStationDTO);

        var expectedCount = before_count + 1;
        var firstCount = context.ProductionStations.Count();
        Assert.Equal(expectedCount, firstCount);

        // update
        var newCode = Util.RandomString(EntityFieldLen.ProductionStation_Code);
        var newName = Util.RandomString(EntityFieldLen.ProductionStation_Name);

        var updatedPayload = await service.SaveProductionStation(new ProductionStationInput {
            Id = result.Payload.Id,
            Code = newCode,
            Name = newName
        });

        var secondCount = context.ProductionStations.Count();
        Assert.Equal(firstCount, secondCount);
        Assert.Equal(newCode, updatedPayload.Payload.Code);
        Assert.Equal(newName, updatedPayload.Payload.Name);
    }


    [Fact]
    public async Task Cannot_add_duplicate_production_station() {
        // setup
        var service = new ProductionStationService(context);

        var code = Util.RandomString(EntityFieldLen.ProductionStation_Code).ToString();
        var name = Util.RandomString(EntityFieldLen.ProductionStation_Name).ToString();

        var count_1 = context.ProductionStations.Count();
        var result = await service.SaveProductionStation(new ProductionStationInput {
            Code = code,
            Name = name
        });

        var count_2 = context.ProductionStations.Count();
        Assert.Equal(count_1 + 1, count_2);

        // insert again
        var result2 = await service.SaveProductionStation(new ProductionStationInput {
            Code = code,
            Name = name
        });


        var count_3 = context.ProductionStations.Count();
        Assert.Equal(count_2, count_3);

        var errorCount = result2.Errors.Count;
        Assert.Equal(2, errorCount);

        var duplicateCode = result2.Errors.Any(e => e.Message == "duplicate code");
        Assert.True(duplicateCode, "expected: 'duplicateion code`");

        var duplicateName = result2.Errors.Any(e => e.Message == "duplicate name");
        Assert.True(duplicateCode, "expected: 'duplicateion name`");
    }
}

