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

        public async Task<MutationPayload<ShipmentOverviewDTO>> ImportShipment(ShipmentInput input) {
            var payload = new MutationPayload<ShipmentOverviewDTO>(null);
            payload.Errors = await ValidateShipmentInput<ShipmentInput>(input);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            // ensure parts
            var partService = new PartService(context);
            List<(string, string)> inputParts = input.Lots
                .SelectMany(t => t.Invoices)
                .SelectMany(t => t.Parts)
                .Select(t => (t.PartNo, t.CustomerPartDesc)).ToList();
            var parts = await partService.GetEnsureParts(inputParts);

            // plant
            var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode);

            // create shipment
            var shipment = new Shipment() {
                Plant = plant,
                Sequence = input.Sequence,
                Lots = input.Lots.Select(lotDTO => new ShipmentLot {
                    LotNo = lotDTO.LotNo,
                    Invoices = lotDTO.Invoices.Select(invoiceDTO => new ShipmentInvoice {
                        InvoiceNo = invoiceDTO.InvoiceNo,
                        ShipDate = invoiceDTO.ShipDate,
                        Parts = invoiceDTO.Parts.Select(partDTO => new ShipmentPart {
                            Part =  parts.First(t => t.PartNo == partDTO.PartNo),
                            Quantity = partDTO.Quantity
                        }).ToList()
                    }).ToList()
                }).ToList()
            };
            context.Shipments.Add(shipment);


            // Add / Update LotPart (s)

            var lotPartInputs = Get_LotPartInputs_from_ShipmentInput(input);
            var lotNumbers = lotPartInputs.Select(t => t.LotNo).Distinct().ToList();
            var lots = await context.VehicleLots
                .Where(t => lotNumbers.Any(lotNo => lotNo == t.LotNo))
                .ToListAsync(); 

            foreach(var lotPartInput in lotPartInputs) {
                var lotPart = await context.LotParts
                    .Where(t => t.Lot.LotNo == lotPartInput.LotNo)
                    .Where(t => t.Part.PartNo == lotPartInput.PartNo)
                    .FirstOrDefaultAsync();

                if (lotPart == null) {
                    lotPart = new LotPart {
                        Lot = lots.First(t => t.LotNo == lotPartInput.LotNo),
                        Part = parts.First(t => t.PartNo == lotPartInput.PartNo),
                    };
                    context.LotParts.Add(lotPart);
                }
                lotPart.BomQuantity = lotPartInput.Quantity;
            }
            

            // save
            await context.SaveChangesAsync();

            payload.Entity = await GetShipmentOverview(shipment.Id);
            return payload;
        }

        public async Task<List<Error>> ValidateShipmentInput<T>(ShipmentInput input) where T : ShipmentInput {
            var errors = new List<Error>();

            var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode);
            if (plant == null) {
                errors.Add(new Error("PlantCode", $"plant not found  {input.PlantCode}"));
                return errors;
            }

            var duplicateShipment = await context.Shipments.AnyAsync(t => t.Plant.Code == input.PlantCode && t.Sequence == input.Sequence);
            if (duplicateShipment) {
                errors.Add(new Error("", $"duplicate shipment plant & sequence found {input.PlantCode} sequence {input.Sequence}"));
                return errors;
            }

            // mossing lot numbers
            var incommingLotNumbers = input.Lots.Select(t => t.LotNo).Distinct().ToList();
            var existingLotNumbers = await context.VehicleLots
                .Where(t => incommingLotNumbers.Any(lotNo => lotNo == t.LotNo))
                .Select(t => t.LotNo)
                .ToListAsync();
            var missingLotNumbers = incommingLotNumbers.Except(existingLotNumbers).ToList();

            if (missingLotNumbers.Count > 0) {
                var missingNumbersStr = String.Join(", ", missingLotNumbers.Take(3));
                errors.Add(new Error("", $"lot number(s) not found {missingNumbersStr}..."));
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

        public async Task<ShipmentOverviewDTO?> GetShipmentOverview(Guid id) {
            return await context.Shipments.Select(t => new ShipmentOverviewDTO {
                Id = t.Id,
                PlantCode = t.Plant.Code,
                Sequence = t.Sequence,
                LotCount = t.Lots.Count(),
                InvoiceCount = t.Lots.SelectMany(t => t.Invoices).Count(),
                PartCount = t.Lots
                    .SelectMany(t => t.Invoices)
                    .SelectMany(t => t.Parts).Select(t => t.Part.PartNo)
                    .Distinct().Count(),
                CreatedAt = t.CreatedAt
            }).FirstOrDefaultAsync(t => t.Id == id);
        }

        public List<LotPartInput> Get_LotPartInputs_from_ShipmentInput(ShipmentInput shipmentInput) {
            return shipmentInput.Lots.Select(t => new {
                LotParts = t.Invoices.SelectMany(u => u.Parts)
                    .Select(u => new {
                        LotNo = t.LotNo,
                        PartNo = u.PartNo,
                        Quantity = u.Quantity
                    })                
                })
                .SelectMany(t => t.LotParts)
                .GroupBy(t => new { t.LotNo, t.PartNo})
                .Select(g => new LotPartInput {
                    LotNo = g.Key.LotNo,
                    PartNo = g.Key.PartNo,
                    Quantity = g.Select(u => u.Quantity).Sum()
                }).ToList();
        }

        public async Task<List<BomShipmentLotPartDTO>> GetShipmentBomPartsCompare(Guid shipmentId) {
            var bomShipmentLotParts = await context.ShipmentParts
                .Where(t => t.ShipmentInvoice.ShipmentLot.Shipment.Id == shipmentId)
                .GroupBy(t => new { t.ShipmentInvoice.ShipmentLot.LotNo, t.Part.PartNo, t.Part.PartDesc  })
                .Select(t => new BomShipmentLotPartDTO {
                    LotNo = t.Key.LotNo,
                    PartNo = t.Key.PartNo,
                    PartDesc = t.Key.PartDesc,
                    ShipmentQuantity = t.Select(u => u.Quantity).Sum()
                }).ToListAsync();


            var lotNumbers = bomShipmentLotParts.Select(t => t.LotNo).ToList();
            
            var bomLotParts = await context.LotParts
                .Where(t => lotNumbers.Any(lotNo => lotNo == t.Lot.LotNo))
                .Select(g => new {
                    LotNo = g.Lot.LotNo,
                    PartNo = g.Part.PartNo,
                    PartDesc = g.Part.PartDesc,
                    Quanity = g.BomQuantity
                })
                .ToListAsync();

            // assign shipment lot part quantity
            bomShipmentLotParts.ForEach(t => {
                t.BomQuantity = bomLotParts
                    .Where(b => b.LotNo == t.LotNo)
                    .Where(b => b.PartNo == t.PartNo)
                    .Select(b => b.Quanity)
                    .FirstOrDefault();
            });

            return bomShipmentLotParts.OrderBy(t => t.LotNo).ThenBy(t => t.PartNo).ToList();
        }
    }
}
