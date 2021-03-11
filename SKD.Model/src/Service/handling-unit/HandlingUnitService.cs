#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.Model {
    public class HandlingUnitService {
        private readonly SkdContext context;

        public HandlingUnitService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<ReceiveHandlingUnitPayload>> SetHandlingUnitReceived(ReceiveHandlingUnitInput input) {
            var payload = new MutationPayload<ReceiveHandlingUnitPayload>(null);
            payload.Errors = await ValidateSetHandlingUnitReceived(input);

            if (payload.Errors.Any()) {
                return payload;
            }

            var handlingUnit = await context.HandlingUnits
                .Include(t => t.Received)
                .Include(t => t.ShipmentInvoice).ThenInclude(t => t.ShipmentLot)
                .FirstOrDefaultAsync(t => t.Code == input.HandlingUnitCode);

            HandlingUnitReceived? handlingUnitReceived = null;
            if (input.Remove) {
                // mark removed
                handlingUnitReceived = handlingUnit.Received.First(t => t.RemovedAt == null);
                handlingUnitReceived.RemovedAt = DateTime.UtcNow;
            } else {
                // add 
                handlingUnitReceived = new HandlingUnitReceived { };
                handlingUnit.Received.Add(handlingUnitReceived);
            }


            await context.SaveChangesAsync();

            payload.Entity = new ReceiveHandlingUnitPayload {
                Code = handlingUnit.Code,
                LotNo = handlingUnit.ShipmentInvoice.ShipmentLot.LotNo,
                InvoiceNo = handlingUnit.ShipmentInvoice.InvoiceNo,
                CreatedAt = handlingUnitReceived.CreatedAt,
                RemovedAt = handlingUnitReceived.RemovedAt
            };

            return payload;
        }

        public async Task<List<Error>> ValidateSetHandlingUnitReceived(ReceiveHandlingUnitInput input) {
            var errors = new List<Error>();

            var handlingUnit = await context.HandlingUnits
                .Include(t => t.Received)
                .FirstOrDefaultAsync(t => t.Code == input.HandlingUnitCode);

            if (handlingUnit == null) {
                errors.Add(new Error("", $"handling unit not found: {input.HandlingUnitCode}"));
                return errors;
            }


            var alreadyReceived = handlingUnit.Received.Any(t => t.RemovedAt == null);

            // if remove then record must exist
            if (input.Remove && !alreadyReceived) {
                errors.Add(new Error("", $"handling unit not yet received"));
                return errors;
            }


            if (!input.Remove && alreadyReceived) {
                errors.Add(new Error("", $"handling unit already recieved: {input.HandlingUnitCode} "));
                return errors;
            } 

            return errors;
        }
    }
}