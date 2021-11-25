namespace SKD.Test;
public class QueryService_Test : TestBase {

    public QueryService_Test() {
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data();
    }

    //     [Fact]
    //     private async Task can_get_lot_parts_by_bom() {
    //         // setup
    //         var bomSequence = 1;
    //         var plant = Gen_Plant();
    //         var lot1 = Gen_LotNo();
    //         var lot2 = Gen_LotNo();

    //         var bomParts = new List<(string LotNo, string PartNo, string PartDesc, int Quantity)> {
    //             (lot1, "part_1", "part_1_desc",  3),
    //             (lot2, "part_2", "part_2_desc",  4),
    //         };

    //         var shipmentParts = new List<(string LotNo, string invoiceNo, string PartNo, string PartDesc, int Quantity)> {
    //             (lot1,  "invoice_1", "part_1", "part_1_desc",  1),
    //             (lot1,  "invoice_2", "part_1", "part_1_desc",  2),

    //             (lot2,  "invoice_3", "part_2", "part_2_desc",  3),
    //             (lot2,  "invoice_4", "part_2", "part_2_desc",  1),
    //         };


    //         var bomLotInput = new BomLotPartInput() {
    //             Sequence = bomSequence,
    //             PlantCode = plant.Code,
    //             LotParts = bomParts.Select(t => new BomLotPartInput.LotPart {
    //                 LotNo = t.LotNo,
    //                 PartNo = t.PartNo,
    //                 PartDesc = t.PartDesc,
    //                 Quantity = t.Quantity

    //             }).ToList()
    //         };

    //         var service = new BomService(ctx);
    //         var bomPayload = await service.ImportBomLotParts(bomLotInput);

    //         // shipments
    //         var shipmentInput = new ShipmentInput() {
    //             PlantCode = plant.Code,
    //             Sequence = 1,
    //             Lots = shipmentParts
    //                 .GroupBy(t => t.LotNo)
    //                 .Select(g1 => new ShipmentLotInput {
    //                     LotNo = g1.Key,
    //                     Invoices = g1.GroupBy(u => u.invoiceNo).Select(g2 => new ShipmentInvoiceInput {
    //                         InvoiceNo = g2.Key,
    //                         Parts = g2.Select(v => new ShipmentPartInput {
    //                             PartNo = v.PartNo,
    //                             CustomerPartNo = v.PartNo,
    //                             CustomerPartDesc = v.PartDesc,
    //                             Quantity = v.Quantity
    //                         }).ToList()
    //                     }).ToList()
    //                 }).ToList()
    //         };

    //         var shipmentService = new ShipmentService(ctx);
    //         await shipmentService.ImportShipment(shipmentInput);

    //         // test by id
    //         var queryService = new QueryService(ctx);
    //         var bomLotPartsCompare = await queryService.GetLotPartsByBom(bomPayload.Entity.Id);

    //         var expected_Lot_parts_count = 2;
    //         var actualCount= bomLotPartsCompare.Count();
    //         Assert.Equal(expected_Lot_parts_count, actualCount);

    //         foreach(var entry in bomLotPartsCompare) {
    //             Assert.Equal(entry.BomQuantity, entry.ShipmentQuantity);
    //         }

    //     }
}
