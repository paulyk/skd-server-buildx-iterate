#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.VCS.Model {

    public class ShipmentService {
        private readonly SkdContext context;

        public ShipmentService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<Shipment>> CreateShipment(ShipmentDTO dto) {
            var shipment = new Shipment() {
                ShipSequenceNo = dto.ShipSequenceNo,
                Lots = dto.Lots.Select(lotDTO => new ShipmentLot {
                    LotNo = lotDTO.LotNo,
                    Invoices = lotDTO.Invoices.Select(invoiceDTO => new ShipmentInvoice {
                        InnvoiceNo = invoiceDTO.InnvoiceNo,
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

            var payload = new MutationPayload<Shipment>(shipment);

            payload.Errors = await ValidateShipmentDTO<ShipmentDTO>(dto);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            context.Shipments.Add(shipment);
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<List<Error>> ValidateShipmentDTO<T>(ShipmentDTO dto) where T : ShipmentDTO {
            var errors = new List<Error>();

            var duplicate = await context.Shipments.AnyAsync(t => t.ShipSequenceNo == dto.ShipSequenceNo);
            if (duplicate) {
                errors.Add(new Error("", "duplicate shipment sequence number"));
                return errors;
            }

            // shipment dto must have lot + invoice + parts
            if (!dto.Lots.Any()) {
                errors.Add(new Error("", "shipment must have lots"));
                return errors;
            }

            if (dto.Lots.Any(t => t.Invoices.Count() == 0)) {
                errors.Add(new Error("", "shipment lots must have invoices"));
                return errors;
            }

            if (dto.Lots.Any(t => t.Invoices.Any(u => u.Parts.Count() == 0))) {
                errors.Add(new Error("", "shipment invoices must have parts"));
                return errors;
            }

            // quantity >= 0
            if (dto.Lots.Any(t => t.Invoices.Any(u => u.Parts.Any(p => p.Quantity <= 0)))) {
                errors.Add(new Error("", "shipment part quanty cannot be <= 0"));
                return errors;
            }

            return errors;
        }
    }
}
