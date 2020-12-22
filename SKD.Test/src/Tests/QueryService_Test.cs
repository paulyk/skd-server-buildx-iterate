using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class QueryService_Test : TestBase {

        public QueryService_Test() {
            ctx = GetAppDbContext();
            Gen_Baseline_Test_Seed_Data();
        }

        [Fact]
        private async Task can_get_bom_shipment_parts_by_lot_comparison() {
            // setup
            var bomSequence = 1;
            var plant = Gen_Plant();
            var lot1 = Gen_LotNo();
            var lot2 = Gen_LotNo();

            var bomParts = new List<(string LotNo, string PartNo, string PartDesc, int Quantity)> {
                (lot1, "part_1", "part_1_desc",  3),
                (lot2, "part_2", "part_2_desc",  4),
            };

            var shipmentParts = new List<(string LotNo, string invoiceNo, string PartNo, string PartDesc, int Quantity)> {
                (lot1,  "inv_1", "part_1", "part_1_desc",  1),
                (lot1,  "inv_2", "part_1", "part_1_desc",  2),

                (lot2,  "inv_3", "part_2", "part_2_desc",  3),
                (lot2,  "inv_4", "part_2", "part_2_desc",  1),

            };


            var dto = new BomLotPartInput() {
                Sequence = bomSequence,
                PlantCode = plant.Code,
                LotParts = bomParts.Select(t => new BomLotPartInput.LotPart {
                    LotNo = t.LotNo,
                    PartNo = t.PartNo,
                    PartDesc = t.PartDesc,
                    Quantity = t.Quantity

                }).ToList()
            };

            var service = new BomService(ctx);
            var bomPayload = await service.ImportBomLotParts(dto);

            // shipments
            var shipmentInput = new ShipmentInput() {
                PlantCode = plant.Code,
                Sequence = 1,
                Lots = shipmentParts
                    .GroupBy(t => t.LotNo)
                    .Select(g1 => new ShipmentLotInput {
                        LotNo = g1.Key,
                        Invoices = g1.GroupBy(u => u.invoiceNo).Select(g2 => new ShipmentInvoiceInput {
                            InvoiceNo = g2.Key,
                            Parts = g2.Select(v => new ShipmentPartInput {
                                PartNo = v.PartNo,
                                CustomerPartNo = v.PartNo,
                                CustomerPartDesc = v.PartDesc,
                                Quantity = v.Quantity
                            }).ToList()
                        }).ToList()
                    }).ToList()
            };

            var shipmentService = new ShipmentService(ctx);
            await shipmentService.ImportShipment(shipmentInput);

            // test
            var queryService = new QueryService(ctx);
            var lotParts = await queryService.GetBomShipmentPartsCompareByBomId(bomPayload.Entity.Id);

            var expectedCount = 2;
            var actualCount= lotParts.Count();
            Assert.Equal(expectedCount, actualCount);

            foreach(var entry in lotParts) {
                Assert.Equal(entry.BomQuantity, entry.ShipmentQuantity);
            }
        }
    }
}