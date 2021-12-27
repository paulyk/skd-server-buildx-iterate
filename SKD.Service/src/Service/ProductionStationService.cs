#nullable enable

namespace SKD.Service;

public class ProductionStationService {
    private readonly SkdContext context;

    public ProductionStationService(SkdContext ctx) {
        this.context = ctx;
    }

    public async Task<MutationResult<ProductionStation>> SaveProductionStation(ProductionStationInput input) {
        var productionStation = await context.ProductionStations.FirstOrDefaultAsync(t => t.Id == input.Id);

        if (productionStation != null) {
            productionStation.Code = input.Code;
            productionStation.Name = input.Name;
        } else {
            productionStation = new ProductionStation { Code = input.Code, Name = input.Name };
            context.ProductionStations.Add(productionStation);
        }
        Trim.TrimStringProperties<ProductionStation>(productionStation);

        MutationResult<ProductionStation> result = new(productionStation);

        // validate
        result.Errors = await ValidateCreateProductionStation<ProductionStation>(productionStation);
        if (result.Errors.Any()) {
            return result;
        }

        // save
        await context.SaveChangesAsync();

        result.Payload = productionStation;
        return result;
    }

    public async Task<List<Error>> ValidateCreateProductionStation<T>(ProductionStation productionStation) where T : ProductionStation {
        var errors = new List<Error>();

        if (productionStation.Code.Trim().Length == 0) {
            errors.Add(ErrorHelper.Create<T>(t => t.Code, "code requred"));
        } else if (productionStation.Code.Length > EntityFieldLen.ProductionStation_Code) {
            errors.Add(ErrorHelper.Create<T>(t => t.Code, $"exceeded code max length of {EntityFieldLen.ProductionStation_Code} characters "));
        }
        if (productionStation.Name.Trim().Length == 0) {
            errors.Add(ErrorHelper.Create<T>(t => t.Name, "name required"));
        } else if (productionStation.Code.Length > EntityFieldLen.ProductionStation_Name) {
            errors.Add(ErrorHelper.Create<T>(t => t.Code, $"exceeded name max length of {EntityFieldLen.ProductionStation_Name} characters "));
        }

        if (await context.ProductionStations.AnyAsync(t => t.Id != productionStation.Id && t.Code == productionStation.Code)) {
            errors.Add(ErrorHelper.Create<T>(t => t.Code, "duplicate code"));
        }
        if (await context.ProductionStations.AnyAsync(t => t.Id != productionStation.Id && t.Name == productionStation.Name)) {
            errors.Add(ErrorHelper.Create<T>(t => t.Name, "duplicate name"));
        }

        return errors;
    }
}

