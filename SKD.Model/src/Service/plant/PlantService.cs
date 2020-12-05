#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.Model {

    public class PlantService {
        private readonly SkdContext context;

        public PlantService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<PlantOverviewDTO>> CreatePlant(PlantInput input) {
            var paylaod = new MutationPayload<PlantOverviewDTO>(null);
            paylaod.Errors = await ValidateCreatePlant(input);
            if (paylaod.Errors.Any()) {
                return paylaod;
            }

            var plant = new Plant {
                Code = input.Code,
                Name = input.Name
            };
            context.Plants.Add(plant);

            // save
            await context.SaveChangesAsync();
            paylaod.Entity = new PlantOverviewDTO {
                Code = plant.Code,
                Name = plant.Name,
                CreatedAt = plant.CreatedAt
            };
            
            return paylaod;
        }

        public async Task<List<Error>> ValidateCreatePlant(PlantInput input) {
            var errors = new List<Error>();

            if (String.IsNullOrEmpty(input.Code) || input.Code.Length < EntityFieldLen.Plant_Code) {
                errors.Add(new Error("Code", "invalid plant code"));
                return errors;
            }

            if (String.IsNullOrEmpty(input.Name) || input.Name.Length > EntityFieldLen.Plant_Name) {
                errors.Add(new Error("Code", "invalid plant name"));
                return errors;
            }

            var duplicateCode = await context.Plants.AnyAsync(t => t.Code == input.Code);
            if (duplicateCode) {
                errors.Add(new Error("Code", "dupicate plant code"));
                return errors;
            }


            var duplicateName = await context.Plants.AnyAsync(t => t.Name == input.Name);
            if (duplicateName) {
                errors.Add(new Error("Code", "dupicate plant name"));
                return errors;
            }

            return errors;

        }
    }
}
