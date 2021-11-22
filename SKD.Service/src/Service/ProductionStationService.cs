#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SKD.Common;
using SKD.Model;

namespace SKD.Service;

public class ProductionStationService {
    private readonly SkdContext context;

    public ProductionStationService(SkdContext ctx) {
        this.context = ctx;
    }

    public async Task<MutationPayload<ProductionStation>> SaveProductionStation(ProductionStationInput input) {
        var productionStation = await context.ProductionStations.FirstOrDefaultAsync(t => t.Id == input.Id);

        if (productionStation != null) {
            productionStation.Code = input.Code;
            productionStation.Name = input.Name;
        } else {
            productionStation = new ProductionStation { Code = input.Code, Name = input.Name };
            context.ProductionStations.Add(productionStation);
        }
        Trim.TrimStringProperties<ProductionStation>(productionStation);

        MutationPayload<ProductionStation> payload = new(productionStation);

        // validate
        payload.Errors = await ValidateCreateProductionStation<ProductionStation>(productionStation);
        if (payload.Errors.Any()) {
            return payload;
        }

        // save
        await context.SaveChangesAsync();

        payload.Payload = productionStation;
        return payload;
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

