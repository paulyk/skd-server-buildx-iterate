#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SKD.Model;

namespace SKD.Service{
    public class HandlingUnitService {
        private readonly SkdContext context;

        public HandlingUnitService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<ReceiveHandlingUnitPayload>> SetHandlingUnitReceived(ReceiveHandlingUnitInput input) {
            // HandlingUNit codes in db are all left '0' padded.. 
            // barcode handling units are not zero padding, so we pad them.
            input = input with {
                HandlingUnitCode = input.HandlingUnitCode.PadLeft(EntityFieldLen.HandlingUnit_Code, '0')
            };
            
            MutationPayload<ReceiveHandlingUnitPayload> payload = new ();
            payload.Errors = await ValidateSetHandlingUnitReceived(input);

            if (payload.Errors.Any()) {
                return payload;
            }

            var handlingUnit = await context.HandlingUnits
                .Include(t => t.Received)
                .Include(t => t.ShipmentInvoice).ThenInclude(t => t.ShipmentLot).ThenInclude(t => t.Lot)
                .FirstAsync(t => t.Code == input.HandlingUnitCode);

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

            payload.Payload = new ReceiveHandlingUnitPayload {
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
                    t.ShipmentInvoice.ShipmentLot.Lot.LotNo,
                    t.ShipmentInvoice.InvoiceNo,
                    PartCount = t.Parts.Where(t => t.RemovedAt == null).Count(),
                    t.CreatedAt,
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

        public async Task<HandlingUnitInfoPayload?> GetHandlingUnitInfo(
            string code
        ) {
            // left pad zeros
            code = code.PadLeft(EntityFieldLen.HandlingUnit_Code, '0');

            var result =
               await (from hu in context.HandlingUnits
                      join lot in context.Lots
                       on hu.ShipmentInvoice.ShipmentLot.Lot.LotNo equals lot.LotNo
                      join model in context.VehicleModels
                       on lot.ModelId equals model.Id
                      where hu.Code == code
                      select new HandlingUnitInfoPayload {
                          Code = hu.Code,
                          PlantCode = hu.ShipmentInvoice.ShipmentLot.Shipment.Plant.Code,
                          ShipmentId =hu.ShipmentInvoice.ShipmentLot.Shipment.Id,
                          ShipmentSequence = hu.ShipmentInvoice.ShipmentLot.Shipment.Sequence,
                          InvoiceNo = hu.ShipmentInvoice.InvoiceNo,
                          LotNo = lot.LotNo,
                          ModelCode = model.Code,
                          ModelName = model.Description,
                          PartCount = hu.Parts.Count,
                          Parts = hu.Parts.Select(p => new HU_Part{
                              PartNo = p.Part.PartNo,
                              PartDesc = p.Part.PartDesc,
                              Quantity = p.Quantity
                              
                          }).ToList()
                      }).FirstAsync();

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