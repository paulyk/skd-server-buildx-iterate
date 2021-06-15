using System;
using System.Collections.Generic;
using SKD.Service;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class LotPartService_Test : TestBase {

        public LotPartService_Test() {
            context = GetAppDbContext();
            Gen_Baseline_Test_Seed_Data(generateLot: false);
        }

        [Fact]
        public async Task can_add_lot_part_quantity_received() {
            // setup
            var plant = Gen_Plant();

            var bomLotPartInput = Gen_BomLotPartInput(plant.Code);
            var bomService = new LotService(context);
            await bomService.ImportBomLotParts(bomLotPartInput);

            var shipmentInput = Gen_ShipmentInput_From_BomLotPartInput(bomLotPartInput);
            var shipmetService = new ShipmentService(context);
            var shipment_payload = await shipmetService.ImportShipment(shipmentInput);

            // test
            var LotPartService = new LotPartService(context);
            foreach (var lotPart in bomLotPartInput.LotParts) {
                var lotPartInput = new ReceiveLotPartInput {
                    LotNo = lotPart.LotNo,
                    PartNo = lotPart.PartNo,
                    Quantity = lotPart.Quantity
                };

                await LotPartService.CreateLotPartQuantityReceived(lotPartInput);
            }

            // Assert
            var lot_parts_received_count = await context.LotPartsReceived.CountAsync();
            Assert.Equal(bomLotPartInput.LotParts.Count, lot_parts_received_count);

            foreach (var lotPart in bomLotPartInput.LotParts) {
                var db_LotPart = await context.LotParts
                    .Where(t => t.Lot.LotNo == lotPart.LotNo)
                    .Where(t => t.Part.PartNo == lotPart.PartNo)
                    .FirstOrDefaultAsync();

                Assert.Equal(lotPart.Quantity, db_LotPart.BomQuantity);
            }
        }

        [Fact]
        public async Task can_replace_lot_part_quantity_received() {
            // setup
            var plant = Gen_Plant();

            var bomLotPartInput = Gen_BomLotPartInput(plant.Code);
            var bomService = new LotService(context);
            await bomService.ImportBomLotParts(bomLotPartInput);

            var shipmentInput = Gen_ShipmentInput_From_BomLotPartInput(bomLotPartInput);
            var shipmetService = new ShipmentService(context);
            var shipment_payload = await shipmetService.ImportShipment(shipmentInput);

            var firstLotPart = bomLotPartInput.LotParts.First();
            var lotPartInput = new ReceiveLotPartInput {
                LotNo = firstLotPart.LotNo,
                PartNo = firstLotPart.PartNo,
                Quantity = firstLotPart.Quantity + 1
            };

            // test
            var service = new LotPartService(context);
            await service.CreateLotPartQuantityReceived(lotPartInput);

            //  change quantity and save
            lotPartInput.Quantity = firstLotPart.Quantity;
            await service.CreateLotPartQuantityReceived(lotPartInput);

            // assert
            var lotPart = await context.LotParts.Include(t => t.Received)
                .Where(t => t.Lot.LotNo == lotPartInput.LotNo)
                .Where(t => t.Part.PartNo == lotPartInput.PartNo)
                .FirstOrDefaultAsync();

            // lot part received entries
            var recevied_count = lotPart.Received.Count();
            var expected_received_count = 2;
            Assert.Equal(recevied_count, expected_received_count);

            // one removed
            var removed_at_count = lotPart.Received.Where(t => t.RemovedAt != null).Count();
            var expected_removed_at_count = 1;
            Assert.Equal(expected_removed_at_count, removed_at_count);

            // quantity
            var latest = lotPart.Received.OrderByDescending(t => t.CreatedAt).First();
            Assert.Equal(firstLotPart.Quantity, latest.Quantity);
        }

        [Fact]
        public async Task cannot_add_duplicate_lot_part_quantity_received() {
            var plant = Gen_Plant();

            var bomLotPartInput = Gen_BomLotPartInput(plant.Code);
            var bomService = new LotService(context);
            await bomService.ImportBomLotParts(bomLotPartInput);

            var shipmentInput = Gen_ShipmentInput_From_BomLotPartInput(bomLotPartInput);
            var shipmetService = new ShipmentService(context);
            var shipment_payload = await shipmetService.ImportShipment(shipmentInput);

            var firstLotPart = bomLotPartInput.LotParts.First();
            var lotPartInput = new ReceiveLotPartInput {
                LotNo = firstLotPart.LotNo,
                PartNo = firstLotPart.PartNo,
                Quantity = firstLotPart.Quantity + 1
            };

            // test
            var service = new LotPartService(context);
            var payload_1 = await service.CreateLotPartQuantityReceived(lotPartInput);
            var payload_2 = await service.CreateLotPartQuantityReceived(lotPartInput);

            var expected_error_message = "duplicate received lot + part + quantity";
            var error_message = payload_2.Errors.Select(t => t.Message).FirstOrDefault();

            Assert.Equal(expected_error_message, error_message);
        }


        private ShipmentInput Gen_ShipmentInput_From_BomLotPartInput(BomLotPartDTO bomLotPartInput) {
            return new ShipmentInput {
                PlantCode = bomLotPartInput.PlantCode,
                Sequence = 1,
                Lots = bomLotPartInput.LotParts.Select(t => new ShipmentLotInput {
                    LotNo = t.LotNo,
                    Invoices = new List<ShipmentInvoiceInput> {
                        new ShipmentInvoiceInput {
                            InvoiceNo = Gen_ShipmentInvoiceNo(),
                            Parts = new List<ShipmentPartInput> {
                                new ShipmentPartInput {
                                    PartNo = t.PartNo,
                                    CustomerPartDesc = t.PartDesc,
                                    Quantity = t.Quantity
                                }
                            }
                        }
                    }
                }).ToList()
            };
        }
        private BomLotPartDTO Gen_BomLotPartInput(string plantCode) {
            return new BomLotPartDTO() {
                Sequence = 1,
                PlantCode = plantCode,
                LotParts = new List<BomLotPartDTO.BomLotPartItem> {
                    new BomLotPartDTO.BomLotPartItem {
                        LotNo = Gen_LotNo(1),
                        PartNo = Gen_PartNo(),
                        PartDesc = Gen_PartDesc(),
                        Quantity = 2
                    },
                    new BomLotPartDTO.BomLotPartItem {
                        LotNo = Gen_LotNo(2),
                        PartNo = Gen_PartNo(),
                        PartDesc = Gen_PartDesc(),
                        Quantity = 3
                    },
                    new BomLotPartDTO.BomLotPartItem {
                        LotNo = Gen_LotNo(3),
                        PartNo = Gen_PartNo(),
                        PartDesc = Gen_PartDesc(),
                        Quantity = 4
                    }
                }
            };

        }

    }
}
