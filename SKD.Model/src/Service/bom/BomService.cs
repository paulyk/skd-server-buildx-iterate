#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.Model {

    public class BomService {
        private readonly SkdContext context;

        public BomService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<BomOverviewDTO>> ImportBomLotParts(BomLotPartsInput input) {
            var payload = new MutationPayload<BomOverviewDTO>(null);
            payload.Errors = await ValidateBomLotPartsInput<BomLotPartsInput>(input);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode);

            var bom = await context.Boms.FirstOrDefaultAsync(t => t.Plant.Code == input.PlantCode && t.Sequence == input.Sequence);
            if (bom == null) {
                bom = new Bom {
                    Plant = plant,
                    Sequence = input.Sequence
                };
                context.Boms.Add(bom);
            }

            foreach (var lotGroup in input.LotParts.GroupBy(t => t.LotNo)) {

                var lot = await context.VehicleLots.FirstOrDefaultAsync(t => t.LotNo == lotGroup.Key);
                if (lot == null) {
                    lot = new VehicleLot {
                        LotNo = lotGroup.Key,
                        Plant = plant,
                    };
                }
                bom.Lots.Add(lot);

                foreach (var entry in lotGroup) {
                    var lotPart = new LotPart {
                        PartNo = entry.PartNo,
                        PartDesc = entry.PartDesc,
                        Quantity = entry.Quantity
                    };
                    lot.LotParts.Add(lotPart);
                }
            }

            await context.SaveChangesAsync();
            payload.Entity = await GetBomOverview(bom.Id);
            return payload;
        }

        public async Task<List<Error>> ValidateBomLotPartsInput<T>(BomLotPartsInput input) where T : BomLotPartsInput {
            var errors = new List<Error>();

            var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode);
            if (plant == null) {
                errors.Add(new Error("PlantCode", $"plant not found  {input.PlantCode}"));
                return errors;
            }

            if (!input.LotParts.Any()) {
                errors.Add(new Error("", "no lot parts found"));
                return errors;
            }

            // duplicate lotNo + Part
            var duplicateLotParts = input.LotParts.GroupBy(t => new { t.LotNo, t.PartNo })
                .Any(g => g.Count() > 1);
            if (duplicateLotParts) {
                errors.Add(new Error("", "duplicate Lot + Part number(s)"));
                return errors;
            }

            if (input.LotParts.Any(t => string.IsNullOrEmpty(t.LotNo))) {
                errors.Add(new Error("", "missing lot number(s)"));
                return errors;
            }

            if (input.LotParts.Any(t => string.IsNullOrEmpty(t.PartNo))) {
                errors.Add(new Error("", "missing part number(s)"));
                return errors;
            }

            if (input.LotParts.Any(t => string.IsNullOrEmpty(t.PartDesc))) {
                errors.Add(new Error("", "missing part decription(s)"));
                return errors;
            }

            if (input.LotParts.Any(t => t.Quantity <= 0)) {
                errors.Add(new Error("", "entries with quantity <= 0"));
                return errors;
            }

            return errors;
        }

        public async Task<MutationPayload<BomOverviewDTO>> ImportBomLotKits(BomLotKitInput input) {
            var payload = new MutationPayload<BomOverviewDTO>(null);
            payload.Errors = await ValidateBomLotKitInput<BomLotKitInput>(input);
            if (payload.Errors.Count > 0) {
                return payload;
            }
            var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode);
            var bom = await context.Boms.FirstOrDefaultAsync(t => t.Plant.Code == input.PlantCode && t.Sequence == input.Sequence);
            if (bom == null) {
                bom = new Bom {
                    Plant = plant,
                    Sequence = input.Sequence
                };
                context.Boms.Add(bom);
            }

            foreach (var inputLot in input.Lots) {
                var lot = await context.VehicleLots.FirstOrDefaultAsync(t => t.LotNo == inputLot.LotNo);
                if (lot == null) {
                    lot = new VehicleLot {
                        LotNo = inputLot.LotNo
                    };
                    bom.Lots.Add(lot);
                }
                foreach (var inputKit in inputLot.Kits) {
                    var vehicle = await CreateVehicleKit(inputKit);
                    lot.Vehicles.Add(vehicle);
                }
            }

            await context.SaveChangesAsync();
            payload.Entity = await GetBomOverview(bom.Id);

            return payload;
        }

        private async Task<Vehicle> CreateVehicleKit(BomLotKitInput.Lot.Kit input) {
            var vehicles = new List<Vehicle>();

            var modelId = await context.VehicleModels
                .Where(t => t.Code == input.ModelCode)
                .Select(t => t.Id).FirstOrDefaultAsync();

            var vehicle = new Vehicle {
                ModelId = modelId,
                KitNo = input.KitNo
            };

            var payload = new MutationPayload<Vehicle>(vehicle);

            // ensure vehicle.Model set
            if (vehicle.ModelId != Guid.Empty) {
                vehicle.Model = await context.VehicleModels
                    .Include(t => t.ModelComponents).ThenInclude(t => t.Component)
                    .Include(t => t.ModelComponents).ThenInclude(t => t.ProductionStation)
                    .FirstOrDefaultAsync(t => t.Id == vehicle.ModelId);
            }

            if (vehicle.Model != null) {
                // add vehicle components
                var modelCOmponents = vehicle.Model.ModelComponents.Where(t => t.RemovedAt == null).ToList();

                modelCOmponents.ForEach(mapping => {
                    vehicle.VehicleComponents.Add(new VehicleComponent() {
                        Component = mapping.Component,
                        ProductionStationId = mapping.ProductionStationId,
                        CreatedAt = vehicle.CreatedAt
                    });
                });
            }

            return vehicle;
        }

        public async Task<List<Error>> ValidateBomLotKitInput<T>(BomLotKitInput input) where T : BomLotKitInput {
            var errors = new List<Error>();

            var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode);
            if (plant == null) {
                errors.Add(new Error("PlantCode", $"plant not found  {input.PlantCode}"));
                return errors;
            }

            if (!input.Lots.Any()) {
                errors.Add(new Error("", "no lots found"));
                return errors;
            }

            // duplicate 
            var duplicate_lotNo = input.Lots.GroupBy(t => t.LotNo)
                .Any(g => g.Count() > 1);
            if (duplicate_lotNo) {
                errors.Add(new Error("", "duplicate Lot no"));
                return errors;
            }

            if (input.Lots.Any(t => t.LotNo.Length != EntityFieldLen.Vehicle_LotNo)) {
                errors.Add(new Error("", "invalid / blank lot nos"));
                return errors;
            }

            if (input.Lots.Any(t => t.Kits.Any(k => k.KitNo.Length != EntityFieldLen.Vehicle_KitNo))) {
                errors.Add(new Error("", "invalid / blank kit nos"));
                return errors;
            }

            if (input.Lots.Any(t => t.Kits.Any(k => String.IsNullOrEmpty(k.ModelCode)))) {
                errors.Add(new Error("", "blank model code(s)"));
                return errors;
            }

            // model codes not found
            var incommingModelCodes = input.Lots.SelectMany(t => t.Kits).Select(k => k.ModelCode);
            var systemModelCodes = await context.VehicleModels
                .Where(t => t.RemovedAt == null).Select(t => t.Code).ToListAsync();

            var matchingModelCodes = incommingModelCodes.Intersect(systemModelCodes);
            var missingModelCodes = incommingModelCodes.Except(matchingModelCodes);

            if (missingModelCodes.Any()) {
                errors.Add(new Error("", $"model codes not in system or removed: {String.Join(",", missingModelCodes)}"));
                return errors;
            }

            return errors;
        }


        private async Task<BomOverviewDTO> GetBomOverview(Guid id) {
            var bom = await context.Boms
                .Where(t => t.Id == id)
                .Select(t => new BomOverviewDTO {
                    PlantCode = t.Plant.Code,
                    Sequence = t.Sequence,
                    LotCount = t.Lots.Count(),
                    LotPartCount = t.Lots.Sum(u => u.LotParts.Count()),
                    VehicleCount = t.Lots.Sum(u => u.Vehicles.Count()),
                    CreatedAt = t.CreatedAt
                })
                .FirstOrDefaultAsync();

            return bom;
        }
    }
}
