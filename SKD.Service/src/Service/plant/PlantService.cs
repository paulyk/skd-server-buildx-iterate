#nullable enable

namespace SKD.Service;

public class PlantService {
    private readonly SkdContext context;

    public PlantService(SkdContext ctx) {
        this.context = ctx;
    }

    public async Task<MutationResult<PlantOverviewDTO>> CreatePlant(PlantInput input) {
        MutationResult<PlantOverviewDTO> paylaod = new();
        paylaod.Errors = await ValidateCreatePlant(input);
        if (paylaod.Errors.Any()) {
            return paylaod;
        }

        var plant = new Plant {
            Code = input.Code,
            PartnerPlantCode = input.PartnerPlantCode,
            PartnerPlantType = input.PartnerPlantType,
            Name = input.Name
        };
        context.Plants.Add(plant);

        // save
        await context.SaveChangesAsync();
        paylaod.Payload = new PlantOverviewDTO {
            Code = plant.Code,
            Name = plant.Name,
            CreatedAt = plant.CreatedAt
        };

        return paylaod;
    }

    public async Task<List<Error>> ValidateCreatePlant(PlantInput input) {
        var errors = new List<Error>();

        if (input.Code is null or "" || input.Code.Length < EntityFieldLen.Plant_Code) {
            errors.Add(new Error("Code", "invalid plant code"));
            return errors;
        }

        if (input.Name is null or "" || input.Name.Length > EntityFieldLen.Plant_Name) {
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

        if (String.IsNullOrEmpty(input.PartnerPlantCode)) {
            errors.Add(new Error("Code", "Parnter plant code required"));
            return errors;
        }

        if (String.IsNullOrEmpty(input.PartnerPlantType)) {
            errors.Add(new Error("Code", "Parnter plant type required"));
            return errors;
        }

        return errors;
    }
}

