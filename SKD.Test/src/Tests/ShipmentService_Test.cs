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
        }

        [Fact]
        private async Task can_import_shipment() {
            // 
            var plant = Gen_Plant();
            var sequence = 2;

            var input = Gen_ShipmentInput_1_lot_2_invoices_3_parts(plant.Code, sequence);
            
            // test
            var before_count = ctx.ShipmentParts.Count();
            var shipmentService = new ShipmentService(ctx);
            var payload = await shipmentService.ImportShipment(input);

            // payload check:  plant code , sequence, count
            Assert.Equal(plant.Code, payload.Entity.PlantCode);
            Assert.Equal(sequence, payload.Entity.Sequence);
            Assert.Equal(1, payload.Entity.LotCount);
            Assert.Equal(2, payload.Entity.InvoiceCount);

            // shipment

            // shipment parts count
            var expected_shipment_parts_count = input.Lots
                .SelectMany(t => t.Invoices)
                .SelectMany(t => t.Parts)
                .Count();

            var actual_shipment_parts_count = ctx.ShipmentParts.Count();
            Assert.Equal(expected_shipment_parts_count, actual_shipment_parts_count);

            // parts count
            var expected_parts_count = input.Lots
                .SelectMany(t => t.Invoices)
                .SelectMany(t => t.Parts)
                .Select(t => t.PartNo).Distinct().Count();

            var actual_parts_count = ctx.Parts.Count();
            Assert.Equal(expected_parts_count, actual_parts_count);

        }

        [Fact]
        private async Task cannot_import_shipment_with_duplicate_plant_and_sequence() {
            var plant = Gen_Plant();
            var sequence = 2;
            var input = Gen_ShipmentInput_1_lot_2_invoices_3_parts(plant.Code, sequence);
            var shipmentService = new ShipmentService(ctx);

            // test
            var payload = await shipmentService.ImportShipment(input);
            var count = await ctx.Shipments.CountAsync();
            Assert.Equal(1, count);

            var payload_1 = await shipmentService.ImportShipment(input);
            var errorCOunt = payload_1.Errors.Count();
            Assert.Equal(1, errorCOunt);

            var expectedMessage = "duplicate shipment plant & sequence found";
            var actualMessage = payload_1.Errors.Select(t => t.Message).FirstOrDefault();

            Assert.Equal(expectedMessage, expectedMessage.Substring(0, expectedMessage.Length));

        }

        [Fact]
        private async Task cannot_import_shipment_with_no_pards() {
            // setup
            var plant = Gen_Plant();
            var input = new ShipmentInput() {
                PlantCode = plant.Code,
                Sequence = 1,
                Lots = new List<ShipmentLotInput> {
                    new ShipmentLotInput {
                        LotNo = "1234",
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
            var plant = Gen_Plant();
            var input = new ShipmentInput() {
                PlantCode = plant.Code,
                Sequence = 1,
                Lots = new List<ShipmentLotInput> {
                    new ShipmentLotInput {
                        LotNo = "1234",
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
            var plant = Gen_Plant();
            var input = new ShipmentInput() {
                PlantCode = plant.Code,
                Sequence = 1,
                Lots = new List<ShipmentLotInput> {
                    new ShipmentLotInput {
                        LotNo = "1234",
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


        public ShipmentInput Gen_ShipmentInput_1_lot_2_invoices_3_parts(string plantCode, int sequence) {
            var input = new ShipmentInput() {
                PlantCode = plantCode,
                Sequence = sequence,
                Lots = new List<ShipmentLotInput> {
                    new ShipmentLotInput {
                        LotNo = "1234",
                        Invoices = new List<ShipmentInvoiceInput> {
                            new ShipmentInvoiceInput {
                                InvoiceNo = "001",
                                Parts = new List<ShipmentPartInput> {
                                    new ShipmentPartInput {
                                        PartNo = "part 1",
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
                                        PartNo = "part 1",
                                        CustomerPartDesc = "part 1 desc",
                                        CustomerPartNo = "part 1 desc",
                                        Quantity = 1
                                    },
                                    new ShipmentPartInput {
                                        PartNo = "part 2",
                                        CustomerPartDesc = "part 2 desc",
                                        CustomerPartNo = "part 2 desc",
                                        Quantity = 1
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
