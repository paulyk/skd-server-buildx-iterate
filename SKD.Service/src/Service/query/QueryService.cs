#nullable enable

namespace SKD.Service;

public class QueryService {
    private readonly SkdContext context;

    public QueryService(SkdContext ctx) {
        this.context = ctx;
    }

    public async Task<List<LotPartDTO>> GetLotPartsByBom(Guid bomId) {
        var result = await context.LotParts
            .Where(t => t.Lot.Bom.Id == bomId)
            .Select(t => new LotPartDTO {
                LotNo = t.Lot.LotNo,
                PartNo = t.Part.PartNo,
                PartDesc = t.Part.PartDesc,
                BomQuantity = t.BomQuantity,
                ShipmentQuantity = t.ShipmentQuantity,
                RemovedDate = t.RemovedAt,

                ReceivedDate = t.Received.OrderByDescending(t => t.CreatedAt)
                    .Where(t => t.RemovedAt == null)
                    .Select(t => t.CreatedAt).Any()
                    ? t.Received.OrderByDescending(t => t.CreatedAt)
                        .Where(t => t.RemovedAt == null)
                        .Select(t => t.CreatedAt).First()
                    : null,

                ReceivedQuantity = t.Received.OrderByDescending(t => t.CreatedAt)
                    .Where(t => t.RemovedAt == null)
                    .Select(t => t.Quantity)
                    .FirstOrDefault(),

                ImportDate = t.CreatedAt
            })
            .OrderBy(t => t.LotNo).ThenBy(t => t.PartNo)
            .ToListAsync();

        return result;
    }

    public async Task<List<LotPartDTO>> GetLotPartsByShipment(Guid shipmentId) {
        var lotNumbers = await context.ShipmentLots
            .Where(t => t.Shipment.Id == shipmentId)
            .Select(t => t.Lot.LotNo).Distinct()
            .ToListAsync();

        var result = await context.LotParts
            .Where(t => lotNumbers.Any(LotNo => LotNo == t.Lot.LotNo))
            .Select(t => new LotPartDTO {
                LotNo = t.Lot.LotNo,
                PartNo = t.Part.PartNo,
                PartDesc = t.Part.PartDesc,
                BomQuantity = t.BomQuantity,
                ShipmentQuantity = t.ShipmentQuantity,

                ReceivedDate = t.Received.OrderByDescending(t => t.CreatedAt)
                    .Where(t => t.RemovedAt == null)
                    .Select(t => t.CreatedAt).Any()
                    ? t.Received.OrderByDescending(t => t.CreatedAt)
                        .Where(t => t.RemovedAt == null)
                        .Select(t => t.CreatedAt).First()
                    : null,

                ReceivedQuantity = t.Received.OrderByDescending(t => t.CreatedAt)
                    .Where(t => t.RemovedAt == null)
                    .Select(t => t.Quantity)
                    .FirstOrDefault(),

                ImportDate = t.CreatedAt
            })
            .OrderBy(t => t.LotNo).ThenBy(t => t.PartNo)
            .ToListAsync();

        return result;
    }

}
