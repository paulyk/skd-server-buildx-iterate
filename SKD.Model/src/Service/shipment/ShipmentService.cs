#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.Model {

    public class ShipmentService {
        private readonly SkdContext context;

        public ShipmentService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<ShipmentOverviewDTO>> CreateShipment(ShipmentInput input) {
            var payload = new MutationPayload<ShipmentOverviewDTO>(null);

            payload.Errors = await ValidateShipmentDTO<ShipmentInput>(input);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            var shipment = new Shipment() {
                Sequence = input.Sequence,
                Lots = input.Lots.Select(lotDTO => new ShipmentLot {
                    LotNo = lotDTO.LotNo,
                    Invoices = lotDTO.Invoices.Select(invoiceDTO => new ShipmentInvoice {
                        InvoiceNo = invoiceDTO.InvoiceNo,
                        ShipDate = invoiceDTO.ShipDate,
                        Parts = invoiceDTO.Parts.Select(partDTO => new ShipmentPart {
                            PartNo = partDTO.PartNo,
                            CustomerPartNo = partDTO.CustomerPartNo,
                            CustomerPartDesc = partDTO.CustomerPartDesc,
                            Quantity = partDTO.Quantity
                        }).ToList()
                    }).ToList()
                }).ToList()
            };

            // ensure plant code
            var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode);
            if (plant == null) {
                plant = new Plant { Code = input.PlantCode, Name = input.PlantCode };
                context.Plants.Add(plant);
            }
            plant.Shipments.Add(shipment);

            // save
            await context.SaveChangesAsync();
            payload.Entity = await context.Shipments.Select(t => new ShipmentOverviewDTO {
                Id = t.Id,
                PlantCode = t.Plant.Code,
                Sequence = t.Sequence,
                LotCount = t.Lots.Count(),
                InvoiceCount = t.Lots.Select(t => t.Invoices.Count()).Sum(),
                CreatedAt = t.CreatedAt
            }).FirstOrDefaultAsync(t => t.Id == shipment.Id);
            return payload;
        }

        public async Task<List<Error>> ValidateShipmentDTO<T>(ShipmentInput input) where T : ShipmentInput {
            var errors = new List<Error>();

            if (String.IsNullOrEmpty(input.PlantCode) || input.PlantCode.Length != EntityFieldLen.Plant_Code ) {
                errors.Add(new Error("", "invalid plant code"));
                return errors;
            }

            var duplicateShipment = await context.Shipments.AnyAsync(t => t.Plant.Code == input.PlantCode && t.Sequence == input.Sequence);
            if (duplicateShipment) {
                errors.Add(new Error("", $"duplicate shipment plant & sequence found {input.PlantCode} sequence {input.Sequence}"));
                return errors;
            }

            // shipment dto must have lot + invoice + parts
            if (!input.Lots.Any()) {
                errors.Add(new Error("", "shipment must have lots"));
                return errors;
            }

            if (input.Lots.Any(t => t.Invoices.Count() == 0)) {
                errors.Add(new Error("", "shipment lots must have invoices"));
                return errors;
            }

            if (input.Lots.Any(t => t.Invoices.Any(u => u.Parts.Count() == 0))) {
                errors.Add(new Error("", "shipment invoices must have parts"));
                return errors;
            }

            if (input.Lots.Any(t => t.Invoices.Any(u => u.Parts.Any(p => String.IsNullOrEmpty(p.PartNo))))) {
                errors.Add(new Error("", "shipment partNo cannot be empty"));
                return errors;
            }


            // quantity >= 0
            if (input.Lots.Any(t => t.Invoices.Any(u => u.Parts.Any(p => p.Quantity <= 0)))) {
                errors.Add(new Error("", "shipment part quanty cannot be <= 0"));
                return errors;
            }

            return errors;
        }
    }
}
