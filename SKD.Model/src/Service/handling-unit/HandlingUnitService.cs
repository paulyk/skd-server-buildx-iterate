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
                .Include(t => t.ShipmentInvoice).ThenInclude(t => t.ShipmentLot).ThenInclude(t => t.Lot)
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
                LotNo = handlingUnit.ShipmentInvoice.ShipmentLot.Lot.LotNo,
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

        public async Task<List<HandlingUnitOverview>> GetHandlingUnitOverviews(
            Guid shipmentId
        ) {

            var r = await context.HandlingUnits
                .Where(t => t.ShipmentInvoice.ShipmentLot.Shipment.Id == shipmentId)
                .Select(t => new {
                    PlantCode = t.ShipmentInvoice.ShipmentLot.Shipment.Plant.Code,
                    ShipmentSequence = t.ShipmentInvoice.ShipmentLot.Shipment.Sequence,
                    HandlingUnitCode = t.Code,
                    LotNo = t.ShipmentInvoice.ShipmentLot.Lot.LotNo,
                    InvoiceNo = t.ShipmentInvoice.InvoiceNo,
                    PartCount = t.Parts.Where(t => t.RemovedAt == null).Count(),
                    CreatedAt = t.CreatedAt,
                    ReceiveEntry = t.Received.OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()
                }).ToListAsync();

            return r.Select(r => new HandlingUnitOverview {
                PlantCode = r.PlantCode,
                ShipmentSequence = r.ShipmentSequence,
                HandlingUnitCode = r.HandlingUnitCode,
                LotNo = r.LotNo,
                InvoiceNo = r.InvoiceNo,
                PartCount = r.PartCount,
                CreatedAt = r.CreatedAt,
                ReceivedAt = r.ReceiveEntry != null 
                    ? r.ReceiveEntry.CreatedAt 
                    : (DateTime?)null,
                ReceiveCancledAt = r.ReceiveEntry != null  && r.ReceiveEntry.RemovedAt != null 
                    ? r.ReceiveEntry.RemovedAt 
                    : (DateTime?)null
            }).ToList();
        }

        public async Task<ValidateReceiveHandlingUnitPayload?> GetValidateReceiveHandlingUnit(
            string code
        ) {
            var result =
               await (from hu in context.HandlingUnits
                      join lot in context.Lots
                       on hu.ShipmentInvoice.ShipmentLot.Lot.LotNo equals lot.LotNo
                      join model in context.VehicleModels
                       on lot.ModelId equals model.Id
                      where hu.Code == code
                      select new ValidateReceiveHandlingUnitPayload {
                          Code = hu.Code,
                          InvoiceNo = hu.ShipmentInvoice.InvoiceNo,
                          LotNo = lot.LotNo,
                          ModelCode = model.Code,
                          ModelName = model.Name,
                          PartCount = hu.Parts.Count()
                      }).FirstOrDefaultAsync();

            var received = await context.HandlingUnitReceived
                .OrderByDescending(t => t.CreatedAt)
                .Where(t => t.HandlingUnit.Code == code)
                .FirstOrDefaultAsync();

            if (received != null) {
                result.ReceivedAt = received.RemovedAt == null 
                    ? received.CreatedAt 
                    : (DateTime?)null;
            }

            return result;
        }
    }
}