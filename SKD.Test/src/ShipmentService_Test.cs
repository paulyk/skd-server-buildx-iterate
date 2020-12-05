using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class ShipmentService_Test : TestBase {

        private SkdContext ctx;
        public ShipmentService_Test() {
            ctx = GetAppDbContext();
        }

        [Fact]
        private async Task can_create_shipment() {
            // 
            var plantCode = Gen_PlantCode();
            var sequence = 2;

            var input = Gen_ShipmentInput_1_lot_2_invoices_2_parts(plantCode, sequence);

            var before_count = ctx.ShipmentParts.Count();
            // test
            var shipmentService = new ShipmentService(ctx);
            var payload = await shipmentService.CreateShipment(input);

            // payload check:  plant code , sequence, count
            Assert.Equal(plantCode, payload.Entity.PlantCode);
            Assert.Equal(sequence, payload.Entity.Sequence);
            Assert.Equal(1, payload.Entity.LotCount);
            Assert.Equal(2, payload.Entity.InvoiceCount);

            // assert
            var after_count = ctx.ShipmentParts.Count();
            Assert.Equal(2, after_count);

        }

        [Fact]
        private async Task cannot_create_shipment_with_duplicate_plant_and_sequence() {
            var plantCode = Gen_PlantCode();
            var sequence = 2;
            var input = Gen_ShipmentInput_1_lot_2_invoices_2_parts(plantCode, sequence);
            var shipmentService = new ShipmentService(ctx);

            // test
            var payload = await shipmentService.CreateShipment(input);
            var count = await ctx.Shipments.CountAsync();
            Assert.Equal(1, count);

            var payload_1 = await shipmentService.CreateShipment(input);
            var errorCOunt= payload_1.Errors.Count();
            Assert.Equal(1, errorCOunt);

            var expectedMessage = "duplicate shipment plant & sequence found";
            var actualMessage = payload_1.Errors.Select(t => t.Message).FirstOrDefault();

            Assert.Equal(expectedMessage, expectedMessage.Substring(0, expectedMessage.Length));

        }

        [Fact]
        private async Task cannot_create_shipment_with_no_pards() {
            // setup
            var input = new ShipmentInput() {
                PlantCode = Gen_PlantCode(),
                Sequence = 1,
                Lots = new List<ShipmentLotInput> {
                    new ShipmentLotInput {
                        LotNo = "1234",
                        Invoices = new List<ShipmentInvoiceInput> {
                            new ShipmentInvoiceInput {
                                InvoiceNo = "001",
                                Parts = new List<ShipmentPartDTO>()
                            }
                        }
                    }
                }
            };


            var before_count = ctx.ShipmentParts.Count();
            // test
            var shipmentService = new ShipmentService(ctx);
            var payload = await shipmentService.CreateShipment(input);

            // assert
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            var expectedError = "shipment invoices must have parts";
            Assert.Equal(expectedError, errorMessage);
        }

        [Fact]
        private async Task cannot_create_shipment_invoice_with_no_parts() {
            // setup
            var input = new ShipmentInput() {
                PlantCode = Gen_PlantCode(),
                Sequence = 1,
                Lots = new List<ShipmentLotInput> {
                    new ShipmentLotInput {
                        LotNo = "1234",
                        Invoices = new List<ShipmentInvoiceInput> {
                            new ShipmentInvoiceInput {
                                InvoiceNo = "001",
                                Parts = new List<ShipmentPartDTO>()
                            }
                        }
                    }
                }
            };


            var before_count = ctx.ShipmentParts.Count();
            // test
            var shipmentService = new ShipmentService(ctx);
            var payload = await shipmentService.CreateShipment(input);

            // assert
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            var expectedError = "shipment invoices must have parts";
            Assert.Equal(expectedError, errorMessage);
        }

        [Fact]
        private async Task cannot_create_shipment_lot_with_no_invoices() {
            // setup
            var input = new ShipmentInput() {
                PlantCode = Gen_PlantCode(),
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
            var payload = await shipmentService.CreateShipment(input);

            // assert
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            var expectedError = "shipment lots must have invoices";
            Assert.Equal(expectedError, errorMessage);
        }
    
    
        public ShipmentInput Gen_ShipmentInput_1_lot_2_invoices_2_parts(string plantCode, int sequence) {
             var input = new ShipmentInput() {
                PlantCode = plantCode,
                Sequence = sequence,
                Lots = new List<ShipmentLotInput> {
                    new ShipmentLotInput {
                        LotNo = "1234",
                        Invoices = new List<ShipmentInvoiceInput> {
                            new ShipmentInvoiceInput {
                                InvoiceNo = "001",
                                Parts = new List<ShipmentPartDTO> {
                                    new ShipmentPartDTO {
                                        PartNo = "0001",
                                        CustomerPartDesc = "part 1",
                                        CustomerPartNo = "cust 0001",
                                        Quantity = 1
                                    }
                                }
                            },
                            new ShipmentInvoiceInput {
                                InvoiceNo = "002",
                                Parts = new List<ShipmentPartDTO> {
                                    new ShipmentPartDTO {
                                        PartNo = "0002",
                                        CustomerPartDesc = "part 2",
                                        CustomerPartNo = "cust 0002",
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
