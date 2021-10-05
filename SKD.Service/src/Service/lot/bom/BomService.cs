#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SKD.Common;
using SKD.Model;

namespace SKD.Service {

    public class BomService {
        private readonly SkdContext context;

        public BomService(SkdContext ctx) {
            this.context = ctx;
        }

        ///<summary>
        /// Import vehicle lot and kits associated with a plant and BOM sequence
        ///</summary>
        public async Task<MutationPayload<BomOverviewDTO>> ImportBom(BomFile input) {
            MutationPayload<BomOverviewDTO> payload = new();
            payload.Errors = await ValidateBomFileInput<BomFile>(input);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            var bom = await GetEnsureBom(plantCode: input.PlantCode, sequence: input.Sequence);

            // add lots
            await EnsureLots(input, bom);

            var parts = await GetEnsureParts(input);

            await AddKits(input, bom);

            Add_LotParts(input, bom, parts);

            await context.SaveChangesAsync();
            payload.Payload = await GetBomOverview(bom.Sequence);
            return payload;
        }

        /*
        ///<summary>
        /// Import lot part and quantity associated with a BOM  sequence
        ///</summary>
        public async Task<MutationPayload<BomOverviewDTO>> ImportBomLotParts(BomLotPartDTO input) {
            MutationPayload<BomOverviewDTO> payload = new();
            payload.Errors = await ValidateVehicleLotPartsInput<BomLotPartDTO>(input);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            var parts = await GetEnsureParts_old(input);

            var lots = await GetEnsureLots_old(input);

            Add_LotParts_old(input, lots, parts);

            await context.SaveChangesAsync();
            payload.Payload = await GetBomOverview(lots.First().BomId);
            return payload;
        }
        */

        #region import bom lot helpers

        private async Task<Bom> GetEnsureBom(string plantCode, int sequence) {
            var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == plantCode);
            var bom = await context.Boms
                .Include(t => t.Plant)
                .Include(t => t.Lots).ThenInclude(t => t.Kits)
                .FirstOrDefaultAsync(t => t.Plant.Code == plantCode && t.Sequence == sequence);

            if (bom == null) {
                bom = new Bom {
                    Plant = plant,
                    Sequence = sequence
                };
                context.Boms.Add(bom);
            }
            return bom;
        }

        private async Task EnsureLots(BomFile input, Bom bom) {
            var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode);
            foreach (var lotNo in input.LotEntries.Select(t => t.LotNo)) {
                var lot = bom.Lots.FirstOrDefault(t => t.LotNo == lotNo);
                if (lot == null) {
                    var modelCode = lotNo.Substring(0, EntityFieldLen.VehicleModel_Code);
                    lot = new Lot {
                        LotNo = lotNo,
                        Plant = plant,
                        Model = await context.VehicleModels.FirstOrDefaultAsync(t => t.Code == modelCode)
                    };
                    bom.Lots.Add(lot);
                }
            }
        }

        private async Task<List<Part>> GetEnsureParts(BomFile input) {
            input.LotParts.ToList().ForEach(t => {
                t.PartNo = PartService.ReFormatPartNo(t.PartNo);
            });

            var partService = new PartService(context);
            List<(string, string)> inputParts = input.LotParts
                .Select(t => (t.PartNo, t.PartDesc)).ToList();
            return await partService.GetEnsureParts(inputParts);
        }
        private async Task<List<Part>> GetEnsureParts_old(BomLotPartDTO input) {
            input.LotParts.ToList().ForEach(t => {
                t.PartNo = PartService.ReFormatPartNo(t.PartNo);
            });

            var partService = new PartService(context);
            List<(string, string)> inputParts = input.LotParts
                .Select(t => (t.PartNo, t.PartDesc)).ToList();
            return await partService.GetEnsureParts(inputParts);
        }

        private async Task<List<Lot>> GetEnsureLots_old(BomLotPartDTO input) {
            // determine existing lots and new lot numbers
            var input_LotNos = input.LotParts.Select(t => t.LotNo).Distinct().ToList();

            var existingLots = await context.Lots
                .Include(t => t.Bom)
                .Where(t => input_LotNos.Any(lotNo => lotNo == t.LotNo)).ToListAsync();

            var new_LotNos = input_LotNos.Except(existingLots.Select(t => t.LotNo)).ToList();

            // get kit models for new lots
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


        private void Add_LotParts(
            BomFile input,
            Bom bom,
            IEnumerable<Part> parts
        ) {

            foreach (var lotGroup in input.LotParts.GroupBy(t => t.LotNo)) {
                var lot = bom.Lots.First(t => t.LotNo == lotGroup.Key);
                foreach (var inputLotPart in lotGroup) {

                    var newLotPart = new LotPart {
                        Part = parts.First(t => t.PartNo == inputLotPart.PartNo),
                        BomQuantity = inputLotPart.Quantity
                    };
                    lot.LotParts.Add(newLotPart);
                }
            }
        }

        private void Add_LotParts_old(
            BomLotPartDTO input,
            IEnumerable<Lot> lots,
            IEnumerable<Part> parts
        ) {

            foreach (var lotGroup in input.LotParts.GroupBy(t => t.LotNo)) {
                var lot = lots.First(t => t.LotNo == lotGroup.Key);
                foreach (var inputLotPart in lotGroup) {

                    var newLotPart = new LotPart {
                        Part = parts.First(t => t.PartNo == inputLotPart.PartNo),
                        BomQuantity = inputLotPart.Quantity
                    };
                    lot.LotParts.Add(newLotPart);
                }
            }
        }

        #endregion

        public async Task<List<Error>> ValidateVehicleLotPartsInput<T>(BomLotPartDTO input) where T : BomLotPartDTO {
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
            if (input.LotParts.Any(t => !Validator.Valid_LotNo(t.LotNo))) {
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

        private async Task AddKits(BomFile input, Bom bom) {
            foreach (var inputLot in input.LotEntries) {
                var modelCode = inputLot.Kits.Select(t => t.ModelCode).First();
                var lot = bom.Lots.First(t => t.LotNo == inputLot.LotNo);
                foreach (var inputKit in inputLot.Kits) {
                    var kit = await CreateKit(inputKit);
                    lot.Kits.Add(kit);
                }
            }
        }

        private async Task<Kit> CreateKit(BomFile.BomFileLot.BomFileKit input) {
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

        public async Task<List<Error>> ValidateBomFileInput<T>(BomFile input) where T : BomFile {
            var errors = new List<Error>();

            var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode);
            if (plant == null) {
                errors.Add(new Error("PlantCode", $"plant not found  {input.PlantCode}"));
                return errors;
            }

            if (!input.LotEntries.Any()) {
                errors.Add(new Error("", "no lots found"));
                return errors;
            }

            if (!input.LotParts.Any()) {
                errors.Add(new Error("", "no lot parts found"));
                return errors;
            }

            // kits alread imported
            var newKitNumbers = input.LotEntries.SelectMany(t => t.Kits).Select(t => t.KitNo).ToList();
            var alreadyImportedKitNumbers = await context.Kits
                .AnyAsync(t => newKitNumbers.Any(newKitNo => newKitNo == t.KitNo));

            if (alreadyImportedKitNumbers) {
                errors.Add(new Error("", "kit numbers already imported"));
            }

            // duplicate lot number in lot enties
            var duplicate_lotNo = input.LotEntries.GroupBy(t => t.LotNo)
                .Any(g => g.Count() > 1);
            if (duplicate_lotNo) {
                errors.Add(new Error("", "duplicate Lot numbers in payload"));
                return errors;
            }

            // duplicate lot part number
            var duplicate_lotPart = input.LotParts.GroupBy(t => new { t.LotNo, t.PartNo })
                .Any(g => g.Count() > 1);
            if (duplicate_lotPart) {
                errors.Add(new Error("", "duplicate Lot + Part number(s) in payload"));
                return errors;
            }

            // validate lotNo format
            if (input.LotEntries.Any(t => !Validator.Valid_LotNo(t.LotNo))) {
                errors.Add(new Error("", "lot numbers  with invalid format found"));
                return errors;
            }

            // validate kitNo format
            if (input.LotEntries.Any(t => t.Kits.Any(k => !Validator.Valid_KitNo(k.KitNo)))) {
                errors.Add(new Error("", "kit numbers with invalid format found"));
                return errors;
            }

            // missing model code
            if (input.LotEntries.Any(t => t.Kits.Any(k => k.ModelCode is null or ""))) {
                errors.Add(new Error("", "kits with missing model code found"));
                return errors;
            }

            // model codes not found
            var incommingModelCodes = input.LotEntries.SelectMany(t => t.Kits).Select(k => k.ModelCode);
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
                    LotCount = t.Lots.Count,
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
                    LotCount = t.Lots.Count,
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
                    LotCount = t.Lots.Count,
                    PartCount = t.Lots.SelectMany(u => u.LotParts).Select(u => u.Part).Distinct().Count(),
                    VehicleCount = t.Lots.SelectMany(u => u.Kits).Count(),
                    CreatedAt = t.CreatedAt
                })
                .FirstOrDefaultAsync();

            return bom;
        }

        #region lot note
        public async Task<MutationPayload<Lot>> SetLotNote(LotNoteInput input) {
            MutationPayload<Lot> paylaod = new();
            paylaod.Errors = await ValidateSetLotNote(input);
            if (paylaod.Errors.Any()) {
                return paylaod;
            }
            var lot = await context.Lots.FirstOrDefaultAsync(t => t.LotNo == input.LotNo);
            lot.Note = input.Note;

            await context.SaveChangesAsync();
            paylaod.Payload = lot;
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
