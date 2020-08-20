#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.VCS.Model {

    public class ProductionStationService {
        private readonly SkdContext context;

        public ProductionStationService(SkdContext ctx) {
            this.context = ctx;
        }

       public async Task<MutationPayload<ProductionStation>> SaveProductionStation(ProductionStationDTO dto) {
            var productionStation = await context.ProductionStations.FirstOrDefaultAsync(t => t.Id == dto.Id);

            if (productionStation != null) {
                productionStation.Code = dto.Code;
                productionStation.Name = dto.Name;
            } else {
                productionStation = new ProductionStation { Code = dto.Code, Name = dto.Name };
                context.ProductionStations.Add(productionStation);
            }
            productionStation.TrimStringProperties();

            var payload = new MutationPayload<ProductionStation>(productionStation);

            // validate
            payload.Errors = await ValidateCreateProductionStation<ProductionStation>(productionStation);
            if (payload.Errors.Any()) {
                return payload;
            }

            // save
            await context.SaveChangesAsync();

            payload.Entity = productionStation;
            return payload;
        }

        public async Task<List<Error>> ValidateCreateProductionStation<T>(ProductionStation productionStation) where T : ProductionStation {
            var errors = new List<Error>();

            if (productionStation.Code.Trim().Length == 0) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, "code requred"));
            } else if (productionStation.Code.Length > EntityMaxLen.ProductionStation_Code) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, $"exceeded code max length of {EntityMaxLen.ProductionStation_Code} characters "));
            }
            if (productionStation.Name.Trim().Length == 0) {
                errors.Add(ErrorHelper.Create<T>(t => t.Name, "name required"));
            } else if (productionStation.Code.Length > EntityMaxLen.ProductionStation_Name) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, $"exceeded name max length of {EntityMaxLen.ProductionStation_Name} characters "));
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
}
