using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class ShipmentService_Test : TestBase {

        public ShipmentService_Test() {
            ctx = GetAppDbContext();
            Gen_Baseline_Test_Seed_Data();
        }

        [Fact]
        private async Task can_import_shipment() {
            // 
            var plant = await ctx.Plants.FirstAsync();
            var lot = await ctx.Lots.FirstAsync();
            var sequence = 2;

            var input = Gen_ShipmentInput(plant.Code, lot.LotNo, sequence);

            // test
            var before_count = ctx.ShipmentParts.Count();
            var shipmentService = new ShipmentService(ctx);
            var payload = await shipmentService.ImportShipment(input);

            // payload check:  plant code , sequence, count
            Assert.Equal(plant.Code, payload.Entity.PlantCode);
            Assert.Equal(sequence, payload.Entity.Sequence);
            Assert.Equal(1, payload.Entity.LotCount);
            Assert.Equal(2, payload.Entity.InvoiceCount);

            // shipment parts count
            var expected_shipment_parts_count = input.Lots
                .SelectMany(t => t.Invoices)
                .SelectMany(t => t.Parts)
                .Count();

            var actual_shipment_parts_count = ctx.ShipmentParts.Count();
            Assert.Equal(expected_shipment_parts_count, actual_shipment_parts_count);

            // imported parts count
            var expected_parts_count = input.Lots
                .SelectMany(t => t.Invoices)
                .SelectMany(t => t.Parts)
                .Select(t => t.PartNo).Distinct().Count();

            var actual_parts_count = ctx.Parts.Count();
            Assert.Equal(expected_parts_count, actual_parts_count);

            // lot parts count
            var expected_lot_parts_count = input.Lots
                .Select(t => new {
                    LotParts = t.Invoices
                        .SelectMany(t => t.Parts)
                        .Select(u => new { LotNo = t.LotNo, PartNo = u.PartNo }).Distinct()
                }).SelectMany(t => t.LotParts).Count();

            var actual_lot_parts = ctx.LotParts.Count();
            Assert.Equal(expected_lot_parts_count, actual_lot_parts);
        }


        [Fact]
        private async Task duplicate_invoice_parts_are_summarized_into_one_invoice_part() {
            // 
            var plant = await ctx.Plants.FirstAsync();
            var lot = await ctx.Lots.FirstAsync();
            var sequence = 2;

            var input = Gen_ShipmentInput(plant.Code, lot.LotNo, sequence);
            
            var shipmentLot = input.Lots.First();
            var shipmentInvoice = input.Lots.First().Invoices.First();
            var shipmentPart = shipmentInvoice.Parts.First();
            shipmentInvoice.Parts.Add(shipmentPart);

            // test
            var before_count = ctx.ShipmentParts.Count();
            var shipmentService = new ShipmentService(ctx);
            var payload = await shipmentService.ImportShipment(input);

            // assert
            var shipment_invoice_part = await ctx.ShipmentParts
                .Where(t => t.ShipmentInvoice.ShipmentLot.LotNo == shipmentLot.LotNo)
                .Where(t => t.ShipmentInvoice.InvoiceNo == shipmentInvoice.InvoiceNo)
                .Where(t => t.Part.PartNo == shipmentPart.PartNo)
                .FirstOrDefaultAsync();

            var expecteShipmentPartdQuantity = shipmentPart.Quantity * 2;
            Assert.Equal(expecteShipmentPartdQuantity, shipment_invoice_part.Quantity);

            // expect lot part quancity
            var expected_LotPart_ShipmentQuantity =  shipmentLot.Invoices
                .SelectMany(t => t.Parts)
                .Where(t => t.PartNo == shipmentPart.PartNo).Sum(t => t.Quantity);

            var actual_lotPart_ShipmentQuantity = await ctx.LotParts
                .Where(t => t.Lot.LotNo == shipmentLot.LotNo)
                .Where(t => t.Part.PartNo == shipmentPart.PartNo)
                .Select(t => t.ShipmentQuantity).FirstOrDefaultAsync();

            Assert.Equal(expected_LotPart_ShipmentQuantity, actual_lotPart_ShipmentQuantity);
        }

        [Fact]
        private async Task cannot_import_shipment_with_duplicate_plant_and_sequence() {
            var plant = await ctx.Plants.FirstAsync();
            var lot = await ctx.Lots.FirstAsync();
            var sequence = 2;
            var input = Gen_ShipmentInput(plant.Code, lot.LotNo, sequence);
            var shipmentService = new ShipmentService(ctx);

            // test
            var payload = await shipmentService.ImportShipment(input);
            var shipmentsCount = await ctx.Shipments.CountAsync();
            Assert.Equal(1, shipmentsCount);

            var payload_1 = await shipmentService.ImportShipment(input);
            var errorCOunt = payload_1.Errors.Count();
            Assert.Equal(1, errorCOunt);

            var expectedMessage = "duplicate shipment plant & sequence found";
            var actualMessage = payload_1.Errors.Select(t => t.Message).FirstOrDefault();

            Assert.Equal(expectedMessage, expectedMessage.Substring(0, expectedMessage.Length));
        }

        [Fact]
        private async Task cannot_import_shipment_if_lot_numbers_not_found() {
            var plant = await ctx.Plants.FirstAsync();
            var lotNo = Gen_LotNo();
            var sequence = 2;
            var input = Gen_ShipmentInput(plant.Code, lotNo, sequence);
            var shipmentService = new ShipmentService(ctx);

            // test
            var payload = await shipmentService.ImportShipment(input);

            var actual_error_message = payload.Errors.Select(t => t.Message).FirstOrDefault();
            var expected_message = "lot number(s) not found";
            Assert.Equal(expected_message, actual_error_message.Substring(0, expected_message.Length));
        }

        [Fact]
        private async Task cannot_import_shipment_with_no_pards() {
            // setup
            var plant = await ctx.Plants.FirstAsync();
            var lot = await ctx.Lots.FirstAsync();

            var input = new ShipmentInput() {
                PlantCode = plant.Code,
                Sequence = 1,
                Lots = new List<ShipmentLotInput> {
                    new ShipmentLotInput {
                        LotNo = lot.LotNo,
                        Invoices = new List<ShipmentInvoiceInput> {
                            new ShipmentInvoiceInput {
                                InvoiceNo = "001",
                                Parts = new List<ShipmentPartInput>()
                            }
                        }
                    }
                }
            };


            var before_count = ctx.ShipmentParts.Count();
            // test
            var shipmentService = new ShipmentService(ctx);
            var payload = await shipmentService.ImportShipment(input);

            // assert
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            var expectedError = "shipment invoices must have parts";
            Assert.Equal(expectedError, errorMessage);
        }

        [Fact]
        private async Task cannot_import_shipment_invoice_with_no_parts() {
            // setup
            var plant = await ctx.Plants.FirstAsync();
            var lot = await ctx.Lots.FirstAsync();
            var input = new ShipmentInput() {
                PlantCode = plant.Code,
                Sequence = 1,
                Lots = new List<ShipmentLotInput> {
                    new ShipmentLotInput {
                        LotNo = lot.LotNo,
                        Invoices = new List<ShipmentInvoiceInput> {
                            new ShipmentInvoiceInput {
                                InvoiceNo = "001",
                                Parts = new List<ShipmentPartInput>()
                            }
                        }
                    }
                }
            };

            var before_count = ctx.ShipmentParts.Count();
            // test
            var shipmentService = new ShipmentService(ctx);
            var payload = await shipmentService.ImportShipment(input);

            // assert
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            var expectedError = "shipment invoices must have parts";
            Assert.Equal(expectedError, errorMessage);
        }

        [Fact]
        private async Task cannot_import_shipment_lot_with_no_invoices() {
            // setup
            var plant = await ctx.Plants.FirstAsync();
            var lot = await ctx.Lots.FirstAsync();
            var input = new ShipmentInput() {
                PlantCode = plant.Code,
                Sequence = 1,
                Lots = new List<ShipmentLotInput> {
                    new ShipmentLotInput {
                        LotNo = lot.LotNo,
                        Invoices = new List<ShipmentInvoiceInput>()
                    }
                }
            };

            var before_count = ctx.ShipmentParts.Count();
            // test
            var shipmentService = new ShipmentService(ctx);
            var payload = await shipmentService.ImportShipment(input);

            // assert
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            var expectedError = "shipment lots must have invoices";
            Assert.Equal(expectedError, errorMessage);
        }

        [Fact]
        public async Task shipment_lot_part_to_lotpart_input_works() {
            // setup
            var plant = await ctx.Plants.FirstAsync();
            var lot = await ctx.Lots.FirstAsync();
            var shipmentInput = Gen_ShipmentInput(plant.Code, lot.LotNo, 6);

            // test
            var service = new ShipmentService(ctx);
            var lotPartInputList = service.Get_LotPartInputs_from_ShipmentInput(shipmentInput);

            // assert
            var expected_lot_part_count = 2;
            var actual_lot_part_count = lotPartInputList.Count();
            Assert.Equal(expected_lot_part_count, actual_lot_part_count);
        }

        public ShipmentInput Gen_ShipmentInput(string plantCode, string lotNo, int sequence) {
            var input = new ShipmentInput() {
                PlantCode = plantCode,
                Sequence = sequence,
                Lots = new List<ShipmentLotInput> {
                    new ShipmentLotInput {
                        LotNo = lotNo,
                        Invoices = new List<ShipmentInvoiceInput> {
                            new ShipmentInvoiceInput {
                                InvoiceNo = "001",
                                Parts = new List<ShipmentPartInput> {
                                    new ShipmentPartInput {
                                        PartNo = "part-1",
                                        CustomerPartDesc = "part 1 desc",
                                        CustomerPartNo = "cust 0001",
                                        Quantity = 1
                                    }
                                }
                            },
                            new ShipmentInvoiceInput {
                                InvoiceNo = "002",
                                Parts = new List<ShipmentPartInput> {
                                    new ShipmentPartInput {
                                        PartNo = "part-1",
                                        CustomerPartDesc = "part 1 desc",
                                        CustomerPartNo = "part 1 desc",
                                        Quantity = 3
                                    },
                                    new ShipmentPartInput {
                                        PartNo = "part-2",
                                        CustomerPartDesc = "part 2 desc",
                                        CustomerPartNo = "part 2 desc",
                                        Quantity = 2
                                    }
                                }
                            },
                        }
                    }
                }
            };
            return input;
        }
    }
}
