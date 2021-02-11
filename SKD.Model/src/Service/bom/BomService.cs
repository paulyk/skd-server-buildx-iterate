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

        ///<summary>
        /// Import vehicle lot part and quantity associated with a BOM  sequence
        ///</summary>
        public async Task<MutationPayload<BomOverviewDTO>> ImportBomLotParts(BomLotPartInput input) {
            var payload = new MutationPayload<BomOverviewDTO>(null);
            payload.Errors = await ValidateVehicleLotPartsInput<BomLotPartInput>(input);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode);

            // create / get bom
            var bom = await context.Boms.FirstOrDefaultAsync(t => t.Plant.Code == input.PlantCode && t.Sequence == input.Sequence);
            if (bom == null) {
                bom = new Bom {
                    Plant = plant,
                    Sequence = input.Sequence
                };
                context.Boms.Add(bom);
            }

            // ensure parts
            var partService = new PartService(context);
            List<(string, string)> inputParts = input.LotParts
                .Select(t => (t.PartNo, t.PartDesc)).ToList();
            var parts = await partService.GetEnsureParts(inputParts);

            // add / update lot parts
            foreach (var lotGroup in input.LotParts.GroupBy(t => t.LotNo)) {

                // ensure lot 
                var lot = await context.Lots.FirstOrDefaultAsync(t => t.LotNo == lotGroup.Key);
                if (lot == null) {
                    lot = new Lot {
                        LotNo = lotGroup.Key,
                        Plant = plant,
                    };
                }
                bom.Lots.Add(lot);

                // add / update lot parts
                foreach (var lotGroupItem in lotGroup) {

                    var existingLotPart = await context.LotParts
                        .Where(t => t.Lot.LotNo == lotGroupItem.LotNo)
                        .Where(t => t.Part.PartNo == lotGroupItem.PartNo)
                        .FirstOrDefaultAsync();

                    // if new lotPart or lotPart bom quantity changed then add
                    if (existingLotPart == null || existingLotPart.BomQuantity != lotGroupItem.Quantity) {
                        // add new because null or because BomQuantity changed
                        var newLotPart = new LotPart {
                            Part = parts.First(t => t.PartNo == PartService.ReFormatPartNo(lotGroupItem.PartNo)),
                            BomQuantity = lotGroupItem.Quantity
                        };
                        lot.LotParts.Add(newLotPart);

                        if (existingLotPart != null) {
                            // since a new lot part was added with a different quantity
                            // flag the existing one as REMOVED
                            existingLotPart.RemovedAt = DateTime.UtcNow;
                        }
                    }
                }
            }

            // remove lot that are in the LotParts table, but not in the input paylload 
            foreach (var lotGroup in input.LotParts.GroupBy(t => t.LotNo)) {

                // if part is no longer part of LOT it will not be in the BomLotPartInput
                // for each lot find parts that are not in the corresponding BomLotPartInput and flag them as removed
                var existingLotParts = await context.LotParts
                    .Where(t => t.Lot.LotNo == lotGroup.Key)
                    .Where(t => t.RemovedAt == null)
                    .ToListAsync();

                var removedLotParts = existingLotParts
                    .Where(t => !input.LotParts.Any(i => i.LotNo == t.Lot.LotNo && i.PartNo == t.Part.PartNo))
                    .ToList();

                removedLotParts.ForEach(lotPart => {
                    lotPart.RemovedAt = DateTime.UtcNow;
                });
            }

            await context.SaveChangesAsync();
            payload.Entity = await GetBomOverview(bom.Id);
            return payload;
        }

        ///<summary>
        /// Import vehicle lot and kits associated with a plant and BOM sequence
        ///</summary>
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
                var lot = await context.Lots.FirstOrDefaultAsync(t => t.LotNo == inputLot.LotNo);
                if (lot == null) {
                    lot = new Lot {
                        LotNo = inputLot.LotNo,
                        Plant = plant
                    };
                    bom.Lots.Add(lot);
                }
                foreach (var inputKit in inputLot.Kits) {
                    var vehicle = await CreateVehicleKit(inputKit);
                    lot.Kits.Add(vehicle);
                }
            }

            await context.SaveChangesAsync();
            payload.Entity = await GetBomOverview(bom.Id);
            return payload;
        }

        public async Task<List<Error>> ValidateVehicleLotPartsInput<T>(BomLotPartInput input) where T : BomLotPartInput {
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

            // duplicate lotNo + Part in payload
            var duplicateLotParts = input.LotParts.GroupBy(t => new { t.LotNo, t.PartNo })
                .Any(g => g.Count() > 1);
            if (duplicateLotParts) {
                errors.Add(new Error("", "duplicate Lot + Part number(s) in payload"));
                return errors;
            }

            // validate lotNo format
            var validator = new Validator();
            if (input.LotParts.Any(t => !validator.Valid_LotNo(t.LotNo))) {
                errors.Add(new Error("", "lot numbers with invalid format found"));
                return errors;
            }

            if (input.LotParts.Any(t => t.PartNo is null or "")) {
                errors.Add(new Error("", "entries with missing part number(s)"));
                return errors;
            }

            if (input.LotParts.Any(t => t.PartDesc is null or "")) {
                errors.Add(new Error("", "entries with missing part decription(s)"));
                return errors;
            }

            if (input.LotParts.Any(t => t.Quantity <= 0)) {
                errors.Add(new Error("", "entries with quantity <= 0"));
                return errors;
            }

            return errors;
        }

        private async Task<Kit> CreateVehicleKit(BomLotKitInput.Lot.LotKit input) {
            var vehicles = new List<Kit>();

            var modelId = await context.VehicleModels
                .Where(t => t.Code == input.ModelCode)
                .Select(t => t.Id).FirstOrDefaultAsync();

            var vehicle = new Kit {
                ModelId = modelId,
                KitNo = input.KitNo
            };

            var payload = new MutationPayload<Kit>(vehicle);

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
                    vehicle.KitComponents.Add(new KitComponent() {
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

            // kits alread imported
            var newKitNumbers = input.Lots.SelectMany(t => t.Kits).Select(t => t.KitNo).ToList();
            var alreadyImportedKitNumbers = await context.Kits
                .AnyAsync(t => newKitNumbers.Any(newKitNo => newKitNo == t.KitNo));

            if (alreadyImportedKitNumbers) {
                errors.Add(new Error("", "kit numbers already imported"));
            }

            // duplicate lot number in payload
            var duplicate_lotNo = input.Lots.GroupBy(t => t.LotNo)
                .Any(g => g.Count() > 1);
            if (duplicate_lotNo) {
                errors.Add(new Error("", "duplicate Lot numbers in payload"));
                return errors;
            }

            // validate lotNo format
            var validator = new Validator();
            if (input.Lots.Any(t => !validator.Valid_LotNo(t.LotNo))) {
                errors.Add(new Error("", "lot numbers  with invalid format found"));
                return errors;
            }

            // validate kitNo format
            if (input.Lots.Any(t => t.Kits.Any(k => !validator.Valid_KitNo(k.KitNo)))) {
                errors.Add(new Error("", "kit numbers with invalid format found"));
                return errors;
            }

            // missing model code
            if (input.Lots.Any(t => t.Kits.Any(k => k.ModelCode is null or ""))) {
                errors.Add(new Error("", "kits with missing model code found"));
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


        public async Task<BomOverviewDTO> GetBomOverview(Guid id) {
            var bom = await context.Boms
                .Where(t => t.Id == id)
                .Select(t => new BomOverviewDTO {
                    Id = t.Id,
                    PlantCode = t.Plant.Code,
                    Sequence = t.Sequence,
                    LotCount = t.Lots.Count(),
                    PartCount = t.Lots.SelectMany(u => u.LotParts).Select(u => u.Part).Distinct().Count(),
                    VehicleCount = t.Lots.SelectMany(u => u.Kits).Count(),
                    CreatedAt = t.CreatedAt
                })
                .FirstOrDefaultAsync();

            return bom;
        }

        public async Task<BomOverviewDTO> GetBomLots(Guid id) {
            var bom = await context.Boms
                .Where(t => t.Id == id)
                .Select(t => new BomOverviewDTO {
                    Id = t.Id,
                    PlantCode = t.Plant.Code,
                    Sequence = t.Sequence,
                    LotCount = t.Lots.Count(),
                    PartCount = t.Lots.SelectMany(u => u.LotParts).Select(u => u.Part).Distinct().Count(),
                    VehicleCount = t.Lots.SelectMany(u => u.Kits).Count(),
                    CreatedAt = t.CreatedAt
                })
                .FirstOrDefaultAsync();

            return bom;
        }
    }
}
