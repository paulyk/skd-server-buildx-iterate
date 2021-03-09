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
            var inputMetrics = GetShipmentInputMetrics(input);

            // test
            var before_count = ctx.ShipmentParts.Count();
            var shipmentService = new ShipmentService(ctx);
            var payload = await shipmentService.ImportShipment(input);

            // payload check:  plant code , sequence, count
            Assert.Equal(plant.Code, payload.Entity.PlantCode);
            Assert.Equal(sequence, payload.Entity.Sequence);
            Assert.Equal(inputMetrics.lotCount, payload.Entity.LotCount);
            Assert.Equal(inputMetrics.invoiceCount, payload.Entity.InvoiceCount);
            Assert.Equal(inputMetrics.handlingUnitCount, payload.Entity.HandlingUnitCount);

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
            var actual_lot_parts = ctx.LotParts.Count();
            Assert.Equal(inputMetrics.lotPartCount, actual_lot_parts);

            // handling unit codes
            var handlingUnitCodes = input.Lots
                .SelectMany(t => t.Invoices).SelectMany(t => t.Parts)
                .Select(t => t.HandlingUnitCode).Distinct().ToList();
            
            var matchingHandlingUnits = await ctx.HandlingUnits.Where(t => handlingUnitCodes.Any(code => code == t.Code)).CountAsync();
            Assert.Equal(inputMetrics.handlingUnitCount, matchingHandlingUnits);
        }

        [Fact]
        private async Task duplicate_handling_unit_parts_are_grouped() {
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
            var shipment_handling_unit_parts = await ctx.ShipmentParts
                .Where(t => t.HandlingUnit.ShipmentInvoice.ShipmentLot.LotNo == shipmentLot.LotNo)
                .Where(t => t.HandlingUnit.ShipmentInvoice.InvoiceNo == shipmentInvoice.InvoiceNo)
                .Where(t => t.Part.PartNo == shipmentPart.PartNo)
                .FirstOrDefaultAsync();

            var expecteShipmentPartdQuantity = shipmentPart.Quantity * 2;
            Assert.Equal(expecteShipmentPartdQuantity, shipment_handling_unit_parts.Quantity);

            // expect lot part quancity
            var expected_LotPart_ShipmentQuantity = shipmentLot.Invoices
                .SelectMany(t => t.Parts)
                .Where(t => t.PartNo == shipmentPart.PartNo).Sum(t => t.Quantity);

            var actual_lotPart_ShipmentQuantity = await ctx.LotParts
                .Where(t => t.Lot.LotNo == shipmentLot.LotNo)
                .Where(t => t.Part.PartNo == shipmentPart.PartNo)
                .Select(t => t.ShipmentQuantity).FirstOrDefaultAsync();

            Assert.Equal(expected_LotPart_ShipmentQuantity, actual_lotPart_ShipmentQuantity);
        }

        [Fact]
        private async Task cannot_import_shipment_duplicate_handling_units() {
            var plant = await ctx.Plants.FirstAsync();
            var lot = await ctx.Lots.FirstAsync();
            var sequence = 2;            
            var shipmentService = new ShipmentService(ctx);

            var input_1 = Gen_ShipmentInput(plant.Code, lot.LotNo, sequence);
            var input_2 = Gen_ShipmentInput(plant.Code, lot.LotNo, sequence + 1);
            var inputMetrics = GetShipmentInputMetrics(input_1);

            var payload_1 = await shipmentService.ImportShipment(input_1);

            var actual_error_count = payload_1.Errors.Count();
            Assert.Equal(0, actual_error_count);
            var actual_handling_unit_count = await ctx.HandlingUnits.CountAsync();
            Assert.Equal(actual_handling_unit_count, inputMetrics.handlingUnitCount);

            // test
            var payload_2 = await shipmentService.ImportShipment(input_2);
            var expected_error_message = "handling units already imported";
            string actual_error_message = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
            Assert.Equal(expected_error_message, (actual_error_message ??= "").Substring(0, expected_error_message.Length));
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
            var lotNo = Util.RandomString(EntityFieldLen.LotNo);
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
            var input = Gen_ShipmentInput(plant.Code, lot.LotNo, 6);
            var inputMetrics = GetShipmentInputMetrics(input);

            // test
            var service = new ShipmentService(ctx);
            var lotPartInputList = service.Get_LotPartInputs_from_ShipmentInput(input);

            // assert
            var actual_lot_part_count = lotPartInputList.Count();
            Assert.Equal(inputMetrics.lotPartCount, actual_lot_part_count);
        }

        public record ShipentInputMetrics(
            int lotCount, 
            int invoiceCount, 
            int invoicePartsCount, 
            int partCount,
            int handlingUnitCount,
            int lotPartCount);

        public ShipentInputMetrics GetShipmentInputMetrics(ShipmentInput input) {

            return new ShipentInputMetrics(
               lotCount: input.Lots.Count(),
               invoiceCount: input.Lots.SelectMany(t => t.Invoices).Count(),
               invoicePartsCount: input.Lots.SelectMany(t => t.Invoices).SelectMany(t => t.Parts).Count(),
               partCount:  input.Lots.SelectMany(t => t.Invoices).SelectMany(t => t.Parts).Select(t => t.PartNo).Distinct().Count(),
               handlingUnitCount: input.Lots.SelectMany(t => t.Invoices).SelectMany(t => t.Parts).Select(t => t.HandlingUnitCode).Distinct().Count(),
               lotPartCount: input.Lots.Select(t => new {
                    lotParts = t.Invoices
                        .SelectMany(t => t.Parts)
                        .Select(u => new { t.LotNo, u.PartNo}).Distinct()
                }).SelectMany(t => t.lotParts).Count()
            );
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
                                        HandlingUnitCode = "0000001",
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
                                        HandlingUnitCode = "0000002",
                                        PartNo = "part-1",
                                        CustomerPartDesc = "part 1 desc",
                                        CustomerPartNo = "part 1 desc",
                                        Quantity = 3
                                    },
                                    new ShipmentPartInput {
                                        HandlingUnitCode = "0000002",
                                        PartNo = "part-2",
                                        CustomerPartDesc = "part 2 desc",
                                        CustomerPartNo = "part 2 desc",
                                        Quantity = 2
                                    },
                                    new ShipmentPartInput {
                                        HandlingUnitCode = "0000003",
                                        PartNo = "part-3",
                                        CustomerPartDesc = "part 3 desc",
                                        CustomerPartNo = "part 3 desc",
                                        Quantity = 4
                                    },
                                    new ShipmentPartInput {
                                        HandlingUnitCode = "0000003",
                                        PartNo = "part-4",
                                        CustomerPartDesc = "part 4 desc",
                                        CustomerPartNo = "part 4 desc",
                                        Quantity = 4
                                    },

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
