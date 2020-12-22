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

        public async Task<List<BomShipmentLotPartDTO>> GetBomShipmentPartsCompareByBomId(Guid bomId) {
            var bomShipmentLotParts = await context.LotParts
                .Where(t => t.Lot.Bom.Id == bomId)
                .OrderBy(t => t.PartNo)
                .Select(t => new BomShipmentLotPartDTO {
                    PlantCode = t.Lot.Plant.Code,
                    BomSequence = t.Lot.Bom.Sequence,
                    LotNo = t.Lot.LotNo,
                    PartNo = t.PartNo,
                    PartDesc = t.PartDesc,
                    BomQuantity = t.Quantity
                }).ToListAsync();

            var lotNumbers = bomShipmentLotParts.Select(t => t.LotNo).ToList();

            var shipmentLotParts = await context.ShipmentParts
                .Where(t => lotNumbers.Any(lotNo => lotNo == t.ShipmentInvoice.ShipmentLot.LotNo))
                .GroupBy(t => new { t.ShipmentInvoice.ShipmentLot.LotNo, t.PartNo })
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


        public async Task<List<BomShipmentLotPartDTO>> GetBomShipmentPartsCompareByLotNo(string lotNo) {
            var bomShipmentLotParts = await context.LotParts
                .Where(t => t.Lot.LotNo == lotNo)
                .OrderBy(t => t.PartNo)
                .Select(t => new BomShipmentLotPartDTO {
                    PlantCode = t.Lot.Plant.Code,
                    BomSequence = t.Lot.Bom.Sequence,
                    LotNo = t.Lot.LotNo,
                    PartNo = t.PartNo,
                    PartDesc = t.PartDesc,
                    BomQuantity = t.Quantity
                }).ToListAsync();

            var lotNumbers = bomShipmentLotParts.Select(t => t.LotNo).ToList();

            var shipmentLotParts = await context.ShipmentParts
                .Where(t => lotNumbers.Any(lotNo => lotNo == t.ShipmentInvoice.ShipmentLot.LotNo))
                .GroupBy(t => new { t.ShipmentInvoice.ShipmentLot.LotNo, t.PartNo })
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