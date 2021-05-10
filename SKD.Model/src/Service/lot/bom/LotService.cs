#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.Model {

    public class LotService {
        private readonly SkdContext context;

        public LotService(SkdContext ctx) {
            this.context = ctx;
        }

        ///<summary>
        /// Import lot part and quantity associated with a BOM  sequence
        ///</summary>
        public async Task<MutationPayload<BomOverviewDTO>> ImportBomLotParts(BomLotPartInput input) {
            var payload = new MutationPayload<BomOverviewDTO>(null);
            payload.Errors = await ValidateVehicleLotPartsInput<BomLotPartInput>(input);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            var parts = await GetEnsureParts(input);

            var lots = await GetEnsureLots(input);

            await Add_Update_Remove_LotParts(input, lots, parts);

            await context.SaveChangesAsync();
            payload.Entity = await GetBomOverview(lots.First().BomId);
            return payload;
        }


        #region import bom lot helpers

        private async Task<List<Part>> GetEnsureParts(BomLotPartInput input) {
            input.LotParts.ToList().ForEach(t => {
                t.PartNo = PartService.ReFormatPartNo(t.PartNo);
            });

            var partService = new PartService(context);
            List<(string, string)> inputParts = input.LotParts
                .Select(t => (t.PartNo, t.PartDesc)).ToList();
            return await partService.GetEnsureParts(inputParts);
        }

        private async Task<List<Lot>> GetEnsureLots(BomLotPartInput input) {
            // determine existing lots and new lot numbers
            var input_LotNos = input.LotParts.Select(t => t.LotNo).Distinct().ToList();

            var existingLots = await context.Lots
                .Include(t => t.Bom)
                .Where(t => input_LotNos.Any(lotNo => lotNo == t.LotNo)).ToListAsync();

            var new_LotNos = input_LotNos.Except(existingLots.Select(t => t.LotNo)).ToList();

            // get vehicle models for new lots
            var modelCodes = new_LotNos.Select(t => t.Substring(0, EntityFieldLen.VehicleModel_Code));
            var models = await context.VehicleModels.Where(t => modelCodes.Any(modelCode => t.Code == modelCode)).ToListAsync();

            // validate
            if (existingLots.Select(t => t.BomId).Distinct().Count() > 1) {
                throw new Exception("more than one bom represented in input lots");
            }

            // add bom, or update bom sequence
            var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode);
            var bom = existingLots.Any() ? existingLots.First().Bom : null;
            if (bom != null) {
                if (bom.Sequence != input.Sequence) {
                    bom.Sequence = input.Sequence;
                }
            } else {
                bom = new Bom {
                    Plant = plant,
                    Sequence = input.Sequence
                };
                context.Boms.Add(bom);
            }

            // // new lots            
            var newLots = new List<Lot>();
            if (new_LotNos.Any()) {
                newLots = new_LotNos.Select(lotNo => new Lot {
                    Model = models.First(t => t.Code == lotNo.Substring(0, EntityFieldLen.VehicleModel_Code)),
                    Plant = plant,
                    LotNo = lotNo,
                    Bom = bom
                }).ToList();
                context.Lots.AddRange(newLots);
            }

            return existingLots.Concat(newLots).ToList();
        }

        private async Task Add_Update_Remove_LotParts(
            BomLotPartInput input,
            IEnumerable<Lot> lots,
            IEnumerable<Part> parts
        ) {

            foreach (var lotGroup in input.LotParts.GroupBy(t => t.LotNo)) {
                var lot = lots.First(t => t.LotNo == lotGroup.Key);
                foreach (var inputLotPart in lotGroup) {

                    var existingLotPart = await context.LotParts
                        .Where(t => t.Lot.LotNo == inputLotPart.LotNo)
                        .Where(t => t.Part.PartNo == inputLotPart.PartNo)
                        .FirstOrDefaultAsync();

                    if (existingLotPart == null || existingLotPart.BomQuantity != inputLotPart.Quantity) {
                        // add new because null or because BomQuantity changed
                        var newLotPart = new LotPart {
                            Part = parts.First(t => t.PartNo == inputLotPart.PartNo),
                            BomQuantity = inputLotPart.Quantity,
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

            /*
            "Remove" any lotPart that is in LotPart table,  
            but is not found in the BomLotPartInput payload
            */
            var inputLotNos = input.LotParts.Select(t => t.LotNo).Distinct().ToList();
            var existingLotParts = await context.LotParts
                .Include(t => t.Lot).Include(t => t.Part)
                .Where(t => t.RemovedAt == null)
                .Where(lotPart => inputLotNos.Any(inputLotNo => lotPart.Lot.LotNo == inputLotNo))
                .ToListAsync();

            var removedLotParts = existingLotParts
                .Where(t => !input.LotParts.Any(i => i.LotNo == t.Lot.LotNo && i.PartNo == t.Part.PartNo))
                .ToList();

            removedLotParts.ForEach(lotPart => {
                lotPart.RemovedAt = DateTime.UtcNow;
            });

        }

        #endregion

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
            var modelCode = input.Lots.SelectMany(t => t.Kits).Select(t => t.ModelCode).First();
            var model = await context.VehicleModels.FirstOrDefaultAsync(t => t.Code == modelCode);
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
                        Model = model,
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
            payload.Entity = await GetBomOverview(bom.Sequence);
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
            var kits = new List<Kit>();

            var model = await context.VehicleModels
                .Include(t => t.ModelComponents)
                .Where(t => t.Code == input.ModelCode)
                .FirstOrDefaultAsync();


            var kit = new Kit {
                KitNo = input.KitNo
            };

            model.ModelComponents.ToList().ForEach(mapping => {
                kit.KitComponents.Add(new KitComponent() {
                    ComponentId = mapping.ComponentId,
                    ProductionStationId = mapping.ProductionStationId,
                    CreatedAt = kit.CreatedAt
                });
            });

            return kit;
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

        public async Task<BomOverviewDTO> GetBomOverview(int bomSequenceNo) {
            var bom = await context.Boms
                .Where(t => t.Sequence == bomSequenceNo)
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

        #region lot note
        public async Task<MutationPayload<Lot>> SetLotNote(LotNoteInput input) {
            var paylaod = new MutationPayload<Lot>(null);
            paylaod.Errors = await ValidateSetLotNote(input);
            if (paylaod.Errors.Any()) {
                return paylaod;
            }
            var lot = await context.Lots.FirstOrDefaultAsync(t => t.LotNo == input.LotNo);
            lot.Note = input.Note;

            await context.SaveChangesAsync();
            paylaod.Entity = lot;
            return paylaod;
        }

        public async Task<List<Error>> ValidateSetLotNote(LotNoteInput input) {
            var errors = new List<Error>();
            var lot = await context.Lots.FirstOrDefaultAsync(t => t.LotNo == input.LotNo);

            if (lot == null) {
                errors.Add(new Error("LotNo", $"Lot not found {input.LotNo}"));
            }
            return errors;
        }
        #endregion
    }
}