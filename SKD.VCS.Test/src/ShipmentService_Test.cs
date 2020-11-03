using System;
using System.Collections.Generic;
using SKD.VCS.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.VCS.Test {
    public class ShipmentService_Test : TestBase {

        private SkdContext ctx;
        public ShipmentService_Test() {
            ctx = GetAppDbContext();
        }

        [Fact]
        private async Task can_create_shipment() {
            // setup

            var dto = new ShipmentDTO() {
                SequenceNo = "0001",
                Lots = new List<ShipmentLotDTO> {
                    new ShipmentLotDTO {
                        LotNo = "1234",
                        Invoices = new List<ShipmentInvoiceDTO> {
                            new ShipmentInvoiceDTO {
                                InvoiceNo = "001",
                                Parts = new List<ShipmentPartDTO> {
                                    new ShipmentPartDTO {
                                        PartNo = "0001",
                                        CustomerPartDesc = "part 1",
                                        CustomerPartNo = "cust 0001",
                                        Quantity = 1
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var before_count = ctx.ShipmentParts.Count();
            // test
            var shipmentService = new ShipmentService(ctx);
            var payload = await shipmentService.CreateShipment(dto);

            // assert
            var after_count = ctx.ShipmentParts.Count();
            Assert.Equal(1, after_count);
        }

        [Fact]
        private async Task cannot_create_shipment_with_no_pards() {
            // setup
            var dto = new ShipmentDTO() {
                SequenceNo = "0001",
                Lots = new List<ShipmentLotDTO> {
                    new ShipmentLotDTO {
                        LotNo = "1234",
                        Invoices = new List<ShipmentInvoiceDTO> {
                            new ShipmentInvoiceDTO {
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
            var payload = await shipmentService.CreateShipment(dto);

            // assert
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            var expectedError = "shipment invoices must have parts";
            Assert.Equal(expectedError, errorMessage);
        }

        [Fact]
        private async Task cannot_create_shipment_invoice_with_no_parts() {
            // setup
            var dto = new ShipmentDTO() {
                SequenceNo = "0001",
                Lots = new List<ShipmentLotDTO> {
                    new ShipmentLotDTO {
                        LotNo = "1234",
                        Invoices = new List<ShipmentInvoiceDTO> {
                            new ShipmentInvoiceDTO {
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
            var payload = await shipmentService.CreateShipment(dto);

            // assert
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            var expectedError = "shipment invoices must have parts";
            Assert.Equal(expectedError, errorMessage);
        }

        [Fact]
        private async Task cannot_create_shipment_lot_with_no_invoices() {
            // setup
            var dto = new ShipmentDTO() {
                SequenceNo = "0001",
                Lots = new List<ShipmentLotDTO> {
                    new ShipmentLotDTO {
                        LotNo = "1234",
                        Invoices = new List<ShipmentInvoiceDTO>()
                    }
                }
            };


            var before_count = ctx.ShipmentParts.Count();
            // test
            var shipmentService = new ShipmentService(ctx);
            var payload = await shipmentService.CreateShipment(dto);

            // assert
            var errorMessage = payload.Errors.Select(t => t.Message).FirstOrDefault();
            var expectedError = "shipment lots must have invoices";
            Assert.Equal(expectedError, errorMessage);
        }
    }
}
