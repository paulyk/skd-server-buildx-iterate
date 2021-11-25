#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SKD.Model;

namespace SKD.Service {

    public class ShipmentService {
        private readonly SkdContext context;

        public ShipmentService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<ShipmentOverviewDTO>> ImportShipment(ShipFile input) {
            MutationPayload<ShipmentOverviewDTO> payload = new ();
            payload.Errors = await ValidateShipmentInput<ShipFile>(input);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            // ensure parts
            var parts = await GetEnsureParts(input);

            // plant
            var plant = await context.Plants.FirstAsync(t => t.Code == input.PlantCode);

            // add shipment
            var shipment = await AddShipment(input, plant, parts);

            // Add / Update LotPart(s)
            await AddUpdateLotParts(input, parts);

            // save
            await context.SaveChangesAsync();

            payload.Payload = await GetShipmentOverview(shipment.Id);
            return payload;
        }

        private async Task<List<Part>> GetEnsureParts(ShipFile input) {
            input.Lots.SelectMany(t => t.Invoices).SelectMany(t => t.Parts).ToList().ForEach(p => {
                p.PartNo = PartService.ReFormatPartNo(p.PartNo);
            });
            var partService = new PartService(context);
            List<(string, string)> inputParts = input.Lots
                .SelectMany(t => t.Invoices)
                .SelectMany(t => t.Parts)
                .Select(t => (t.PartNo, t.CustomerPartDesc)).ToList();
            return await partService.GetEnsureParts(inputParts);
        }

        private async  Task<Shipment> AddShipment(ShipFile input, Plant plant, List<Part> parts) {
            // lots
            var lotNos = input.Lots.Select(t => t.LotNo).ToList();
            var lots = await context.Lots.Where(t => lotNos.Any(lotNos=> lotNos == t.LotNo)).ToListAsync();

            // create shipment
            var shipment = new Shipment() {
                Plant = plant,
                Sequence = input.Sequence,
                ShipmentLots = input.Lots.Select(lotDTO => new ShipmentLot {
                    Lot = lots.First(t => t.LotNo == lotDTO.LotNo),
                    Invoices = lotDTO.Invoices.Select(invoiceDTO => new ShipmentInvoice {
                        InvoiceNo = invoiceDTO.InvoiceNo,
                        ShipDate = invoiceDTO.ShipDate,
                        HandlingUnits = invoiceDTO.Parts
                            .GroupBy(t => t.HandlingUnitCode)
                            .Select(g => new HandlingUnit {
                                Code = g.Key,
                                Parts = g.GroupBy(p => p.PartNo).Select(u => new ShipmentPart {
                                    Part = parts.FirstOrDefault(t => t.PartNo == u.Key),
                                    Quantity = u.Sum(x => x.Quantity)
                                }).ToList()
                            }).ToList(),
                    }).ToList()
                }).ToList()
            };

            context.Shipments.Add(shipment);
            return shipment;
        }

        private async Task AddUpdateLotParts(ShipFile input, List<Part> parts) {
            var lotPartInputs = Get_LotPartInputs_from_ShipmentInput(input);
            var lotNumbers = lotPartInputs.Select(t => t.LotNo).Distinct().ToList();
            var lots = await context.Lots
                .Where(t => lotNumbers.Any(lotNo => lotNo == t.LotNo))
                .ToListAsync();

            foreach (var lotPartInput in lotPartInputs) {
                var lotPart = await context.LotParts
                    .Where(t => t.Lot.LotNo == lotPartInput.LotNo)
                    .Where(t => t.Part.PartNo == PartService.ReFormatPartNo(lotPartInput.PartNo))
                    .FirstOrDefaultAsync();

                if (lotPart == null) {
                    lotPart = new LotPart {
                        Lot = lots.First(t => t.LotNo == lotPartInput.LotNo),
                        Part = parts.First(t => t.PartNo == PartService.ReFormatPartNo(lotPartInput.PartNo)),
                    };
                    context.LotParts.Add(lotPart);
                }
                lotPart.ShipmentQuantity = lotPartInput.Quantity;
            }
        }

        public async Task<List<Error>> ValidateShipmentInput<T>(ShipFile input) where T : ShipFile {
            var errors = new List<Error>();

            // plant
            var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode);
            if (plant == null) {
                errors.Add(new Error("PlantCode", $"plant not found  {input.PlantCode}"));
                return errors;
            }

            // dupliate shipment plant + sequence
            var duplicateShipment = await context.Shipments.AnyAsync(t => t.Plant.Code == input.PlantCode && t.Sequence == input.Sequence);
            if (duplicateShipment) {
                errors.Add(new Error("", $"duplicate shipment plant & sequence found {input.PlantCode} sequence {input.Sequence}"));
                return errors;
            }

            // mossing Lots
            var incommingLotNumbers = input.Lots.Select(t => t.LotNo).Distinct().ToList();
            var existingLotNumbers = await context.Lots
                .Where(t => incommingLotNumbers.Any(lotNo => lotNo == t.LotNo))
                .Select(t => t.LotNo)
                .ToListAsync();
            var missingLotNumbers = incommingLotNumbers.Except(existingLotNumbers).ToList();

            if (missingLotNumbers.Count > 0) {
                var missingNumbersStr = String.Join(", ", missingLotNumbers.Take(3));
                errors.Add(new Error("", $"lot number(s) not found {missingNumbersStr}..."));
                return errors;
            }

            // shipment input must have lot + invoice + handling_units + parts
            if (!input.Lots.Any()) {
                errors.Add(new Error("", "shipment must have lots"));
                return errors;
            }

            // lots must have invoices
            if (input.Lots.Any(t => !t.Invoices.Any())) {
                errors.Add(new Error("", "shipment lots must have invoices"));
                return errors;
            }

            // inoices must have parts
            if (input.Lots.Any(t => t.Invoices.Any(u => u.Parts.Count == 0))) {
                errors.Add(new Error("", "shipment invoices must have parts"));
                return errors;
            }

            // handling unit code required
            if (input.Lots.Any(t => t.Invoices.Any(u => u.Parts.Any(p => p.HandlingUnitCode is null or "")))) {
                errors.Add(new Error("", "shipment handling unit code cannot be empty"));
                return errors;
            }

            // inoice numbers alread exists
            var inputInvoiceNos = input.Lots
                .SelectMany(t => t.Invoices)
                .Select(t => t.InvoiceNo).Distinct().ToList();

            var invoicesAlreadyImorted = await context.ShipmentInvoices
                .Where(t => inputInvoiceNos.Any(invoiceNo => invoiceNo == t.InvoiceNo)).Select(t => t.InvoiceNo)
                .ToListAsync();

            if (invoicesAlreadyImorted.Any()) {
                var invoiceNos = String.Join(", ", invoicesAlreadyImorted.Take(5));
                errors.Add(new Error("", $"invoice numbers already imported: {invoiceNos}"));
                return errors;
            }

            // handling units already exist
            var inputHandlingUnitCodes = input.Lots
                .SelectMany(t => t.Invoices)
                .SelectMany(t => t.Parts)
                .Select(p => p.HandlingUnitCode).Distinct().ToList();

            var handlingUnitCodeAlreadyImported = await context.HandlingUnits
                .Where(t => inputHandlingUnitCodes.Any(code => code == t.Code))
                .Select(t => t.Code)
                .ToListAsync();

            if (handlingUnitCodeAlreadyImported.Any()) {
                var codes = String.Join(", ", handlingUnitCodeAlreadyImported.Take(5));
                errors.Add(new Error("", $"handling units already imported: {codes}"));
                return errors;
            }

            // part no required
            if (input.Lots.Any(t => t.Invoices.Any(u => u.Parts.Any(p => p.PartNo is null or "")))) {
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

        public async Task<ShipmentOverviewDTO?> GetShipmentOverview(Guid id) {
            return await context.Shipments.Select(t => new ShipmentOverviewDTO {
                Id = t.Id,
                PlantCode = t.Plant.Code,
                Sequence = t.Sequence,
                LotCount = t.ShipmentLots.Count,
                InvoiceCount = t.ShipmentLots.SelectMany(t => t.Invoices).Count(),

                HandlingUnitCount = t.ShipmentLots.SelectMany(t => t.Invoices).SelectMany(t => t.HandlingUnits).Count(),
                HandlingUnitReceivedCount = t.ShipmentLots.SelectMany(t => t.Invoices).SelectMany(t => t.HandlingUnits)
                    .Where(t => t.Received.Any(t => t.RemovedAt== null)).Count(),                

                LotPartCount = t.ShipmentLots.SelectMany(u => u.Lot.LotParts).Count(),
                LotPartReceivedCount = t.ShipmentLots.SelectMany(u => u.Lot.LotParts)
                    .Where(u => u.Received.Any(y => y.RemovedAt == null)).Count(),

                BomShipDiffCount = t.ShipmentLots.SelectMany(u => u.Lot.LotParts)
                    .Where(u => u.BomQuantity != u.ShipmentQuantity).Count(),

                LotPartReceiveBomDiffCount = t.ShipmentLots
                    .SelectMany(u => u.Lot.LotParts)
                    .Where(lp => lp.Received.Any(r => r.RemovedAt == null && r.Quantity != lp.BomQuantity)).Count(),

                PartCount = t.ShipmentLots
                    .SelectMany(t => t.Invoices)
                    .SelectMany(t => t.HandlingUnits)
                    .SelectMany(t => t.Parts)
                    .Distinct().Count(),
                CreatedAt = t.CreatedAt
            }).FirstOrDefaultAsync(t => t.Id == id);
        }

        public List<LotPartQuantityDTO> Get_LotPartInputs_from_ShipmentInput(ShipFile shipmentInput) {
            return shipmentInput.Lots.Select(t => new {
                LotParts = t.Invoices.SelectMany(u => u.Parts)
                    .Select(u => new {
                        t.LotNo,
                        u.PartNo,
                        u.Quantity
                    })
            })
                .SelectMany(t => t.LotParts)
                .GroupBy(t => new { t.LotNo, t.PartNo })
                .Select(g => new LotPartQuantityDTO {
                    LotNo = g.Key.LotNo,
                    PartNo = g.Key.PartNo,
                    Quantity = g.Select(u => u.Quantity).Sum()
                }).ToList();
        }
    }
}
