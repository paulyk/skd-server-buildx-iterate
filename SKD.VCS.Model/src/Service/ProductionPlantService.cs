#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.VCS.Model {

    public class ProductionPlantService {
        private readonly SkdContext context;

        public ProductionPlantService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<ProductionPlant>> CreateProductionPlant(ProductionPlantDTO dto) {
            var productionPlant = new ProductionPlant {
                Code = dto.Code,
                Name = dto.Name
            };

            var payload = new MutationPayload<ProductionPlant>(productionPlant);

            // validate
            payload.Errors = await ValidateCreateProductionPlant<ProductionPlant>(dto);
            if (payload.Errors.Any()) {
                return payload;
            }

            // save
            context.ProductionPlants.Add(payload.Entity);
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<List<Error>> ValidateCreateProductionPlant<T>(ProductionPlantDTO dto) where T : ProductionPlant {
            var errors = new List<Error>();

            if (String.IsNullOrEmpty(dto.Code)) {
                errors.Add(new Error("code", "code required"));   
            }

            if (String.IsNullOrEmpty(dto.Name)) {
                errors.Add(new Error("name", "name required"));   
            }

            if (await context.ProductionPlants.AnyAsync(t => t.Code == dto.Code)) {
                errors.Add(new Error("code", "duplicate code"));   
            }

            if (await context.ProductionPlants.AnyAsync(t => t.Name == dto.Name)) {
                errors.Add(new Error("code", "duplicate name"));   
            }


            return errors;
        }
    }
}
