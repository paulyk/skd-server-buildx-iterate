#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.Model {

    public class QueryService {
        private readonly SkdContext context;

        public QueryService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<List<LotPartDTO>> GetBomLotParts(Guid bomId) {
            var result = await context.LotParts
                .Where(t => t.Lot.Bom.Id == bomId)
                .Select(t => new LotPartDTO {
                    LotNo = t.Lot.LotNo,
                    PartNo = t.Part.PartNo,
                    PartDesc = t.Part.PartDesc,
                    BomQuantity  = t.BomQuantity,
                    ShipmentQuantity = t.ShipmentQuantity,

                    ReceivedDate = t.Received.OrderByDescending(t => t.CreatedAt)
                        .Where(t => t.RemovedAt == null)
                        .Select(t => t.CreatedAt)
                        .FirstOrDefault(),

                    ReceivedQuantity = t.Received.OrderByDescending(t => t.CreatedAt)
                        .Where(t => t.RemovedAt == null)
                        .Select(t => t.Quantity)
                        .FirstOrDefault(),

                    ImportDate = t.CreatedAt
                })
                .ToListAsync();

            return result;
        }


        public async Task<List<BomShipmentLotPartDTO>> GetBomShipmentPartsCompareByLotNo(string lotNo) {
            var bomShipmentLotParts = await context.LotParts
                .Where(t => t.Lot.LotNo == lotNo)
                .OrderBy(t => t.Part.PartNo)
                .Select(t => new BomShipmentLotPartDTO {
                    LotNo = t.Lot.LotNo,
                    PartNo = t.Part.PartNo,
                    PartDesc = t.Part.PartDesc,
                    BomQuantity = t.BomQuantity
                }).ToListAsync();

            var lotNumbers = bomShipmentLotParts.Select(t => t.LotNo).ToList();

            var shipmentLotParts = await context.ShipmentParts
                .Where(t => lotNumbers.Any(lotNo => lotNo == t.ShipmentInvoice.ShipmentLot.LotNo))
                .GroupBy(t => new { t.ShipmentInvoice.ShipmentLot.LotNo, t.Part.PartNo })
                .Select(g => new {
                    LotNo = g.Key.LotNo,
                    PartNo = g.Key.PartNo,
                    Quanity = g.Select(t => t.Quantity).Sum()
                })
                .ToListAsync();

            // assign shipment lot part quantity
            bomShipmentLotParts.ForEach(blp => {
                blp.ShipmentQuantity = shipmentLotParts
                    .Where(shipPart => shipPart.LotNo == blp.LotNo)
                    .Where(shipPart => shipPart.PartNo == blp.PartNo)
                    .Select(t => t.Quanity)
                    .FirstOrDefault();
            });

            return bomShipmentLotParts;
        }
    }
}