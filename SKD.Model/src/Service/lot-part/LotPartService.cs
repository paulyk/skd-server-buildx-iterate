#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.Model {

    public class LotPartService {
        private readonly SkdContext context;

        public LotPartService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<LotPartDTO>> CreateLotPartQuantityReceived(LotPartInput input) {
            var paylaod = new MutationPayload<LotPartDTO>(null);
            paylaod.Errors = await ValidateCreateLotPartReceipt(input);
            if (paylaod.Errors.Any()) {
                return paylaod;
            }

            var lotPart = await context.LotParts
                .Include(t => t.Received)
                .Where(t => t.Lot.LotNo == input.LotNo)
                .Where(t => t.Part.PartNo == input.PartNo)                
                .FirstOrDefaultAsync();

            // mark existing received records as removed
            lotPart.Received
                .Where(t => t.RemovedAt == null).ToList()
                .ForEach(t => {
                    t.RemovedAt = DateTime.UtcNow;
                });

            // add new received 
            var lotPartReceived = new LotPartReceived {
                LotPart = await context.LotParts
                    .Where(t => t.Lot.LotNo == input.LotNo && t.Part.PartNo == input.PartNo)
                    .FirstOrDefaultAsync(),
                Quantity = input.Quantity
            };
            context.LotPartsReceived.Add(lotPartReceived);

            await context.SaveChangesAsync();

            paylaod.Entity = await GetLotPartOverview(input.LotNo, input.PartNo);
            return paylaod;
        }

        public async Task<List<Error>> ValidateCreateLotPartReceipt(LotPartInput input) {
            var errors = new List<Error>();

            var lot = await context.VehicleLots.FirstOrDefaultAsync(t => t.LotNo == input.LotNo);
            if (lot == null) {
                errors.Add(new Error("LotNo", $"lot not found {input.LotNo}"));
                return errors;
            }

            var part = await context.Parts.FirstOrDefaultAsync(t => t.PartNo == input.PartNo);
            if (lot == null) {
                errors.Add(new Error("PartNo", $"Part not found {input.PartNo.Trim()}"));
                return errors;
            }

            if (input.Quantity < 0) {
                errors.Add(new Error("Quantity", "Quantity less than 0"));
                return errors;
            }

            var lotPart = await context.LotParts
                .Include(t => t.Received)
                .Where(t => t.Lot.LotNo == input.LotNo)
                .Where(t => t.Part.PartNo == input.PartNo)                
                .FirstOrDefaultAsync();
            
            if (lotPart == null) {
                errors.Add(new Error("", "no matching lot + part found in system"));
                return errors;
            }

            var duplicate = lotPart.Received
                .OrderByDescending(t => t.CreatedAt)
                .Where(t => t.RemovedAt == null)
                .Where(t => t.Quantity == input.Quantity)
                .Any();

            if (duplicate == true) {
                errors.Add(new Error("", "duplicate received lot + part + quantity"));
                return errors;
            }

            return errors;
        }

        public async Task<LotPartDTO> GetLotPartOverview(string lotNo, string PartNo) {
            var lotPart = await context.LotParts
                .Where(t => t.Lot.LotNo == lotNo && t.Part.PartNo == PartNo)
                    .Include(t => t.Lot)
                    .Include(t => t.Part)
                    .Include(t => t.Received)
                .FirstOrDefaultAsync();

            var receivedLotPrt = lotPart.Received
                    .OrderByDescending(t => t.CreatedAt)
                    .Where(t => t.RemovedAt == null)
                    .FirstOrDefault();
            
            return new LotPartDTO {
                LotNo = lotPart.Lot.LotNo,
                PartNo = lotPart.Part.PartNo,
                PartDesc = lotPart.Part.PartDesc,
                Quantity = lotPart.Quantity,
                QuantityReceived =  receivedLotPrt != null ? receivedLotPrt.Quantity : 0,
                ImportDate = lotPart.CreatedAt,
                ReceivedDate = receivedLotPrt != null ? receivedLotPrt.CreatedAt : (DateTime?)null
            };
        }
    }
}
