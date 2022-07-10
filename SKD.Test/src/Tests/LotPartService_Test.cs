namespace SKD.Test;

using PartQuantities = IEnumerable<(string partNo, int quantity)>;

public class LotPartService_Test : TestBase {

    public LotPartService_Test() {
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data(generateLot: false);
    }

    [Fact]
    public async Task Can_add_lot_part_quantity_received() {
        // setup
        var plant = Gen_Plant();

        var bomFile = Gen_BomFileInput(plant.Code);
        var bomService = new BomService(context);
        await bomService.ImportBom(bomFile);

        var shipmentInput = Gen_ShipmentInput(bomFile);
        var shipmetService = new ShipmentService(context);
        var shipment_payload = await shipmetService.ImportShipment(shipmentInput);

        // test
        var LotPartService = new LotPartService(context);
        foreach (var lotPart in bomFile.LotParts) {
            var lotPartInput = new ReceiveLotPartInput {
                LotNo = lotPart.LotNo,
                PartNo = lotPart.PartNo,
                Quantity = lotPart.Quantity
            };

            await LotPartService.CreateLotPartQuantityReceived(lotPartInput);
        }

        // Assert
        var lot_parts_received_count = await context.LotPartsReceived.CountAsync();
        Assert.Equal(bomFile.LotParts.Count, lot_parts_received_count);

        foreach (var lotPart in bomFile.LotParts) {
            var db_LotPart = await context.LotParts
                .Where(t => t.Lot.LotNo == lotPart.LotNo)
                .Where(t => t.Part.PartNo == lotPart.PartNo)
                .FirstOrDefaultAsync();

            Assert.Equal(lotPart.Quantity, db_LotPart.BomQuantity);
        }
    }

    [Fact]
    public async Task Can_replace_lot_part_quantity_received() {
        // setup
        var plant = Gen_Plant();

        var bomFile = Gen_BomFileInput(plant.Code);
        var bomService = new BomService(context);
        await bomService.ImportBom(bomFile);

        var shipmentInput = Gen_ShipmentInput(bomFile);
        var shipmetService = new ShipmentService(context);
        var shipment_payload = await shipmetService.ImportShipment(shipmentInput);

        var firstLotPart = bomFile.LotParts.First();
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
        var recevied_count = lotPart.Received.Count;
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
    public async Task Cannot_add_duplicate_lot_part_quantity_received() {
        var plant = Gen_Plant();

        var bomFile = Gen_BomFileInput(plant.Code);
        var bomService = new BomService(context);
        await bomService.ImportBom(bomFile);

        var shipmentInput = Gen_ShipmentInput(bomFile);
        var shipmetService = new ShipmentService(context);
        var shipment_payload = await shipmetService.ImportShipment(shipmentInput);

        var firstLotPart = bomFile.LotParts.First();
        var lotPartInput = new ReceiveLotPartInput {
            LotNo = firstLotPart.LotNo,
            PartNo = firstLotPart.PartNo,
            Quantity = firstLotPart.Quantity + 1
        };

        // test
        var service = new LotPartService(context);
        var result_1 = await service.CreateLotPartQuantityReceived(lotPartInput);
        var result_2 = await service.CreateLotPartQuantityReceived(lotPartInput);

        var expected_error_message = "duplicate received lot + part + quantity";
        var error_message = result_2.Errors.Select(t => t.Message).FirstOrDefault();

        Assert.StartsWith(expected_error_message, error_message);
    }


    private ShipFile Gen_ShipmentInput(BomFile bomFile) {
        return new ShipFile {
            PlantCode = bomFile.PlantCode,
            Sequence = 1,
            Lots = bomFile.LotParts.Select(t => new ShipFileLot {
                LotNo = t.LotNo,
                Invoices = new List<ShipFileInvoice> {
                        new ShipFileInvoice {
                            InvoiceNo = Gen_ShipmentInvoiceNo(),
                            Parts = new List<ShipFilePart> {
                                new ShipFilePart {
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

    private BomFile Gen_BomFileInput(string plantCode) {
        var lotNumbers = new List<string> { Gen_LotNo(1), Gen_LotNo(2), Gen_LotNo(3) };

        return new BomFile() {
            Sequence = 1,
            PlantCode = plantCode,
            LotParts = new List<BomFile.BomFileLotPart> {
                    new BomFile.BomFileLotPart {
                        LotNo = lotNumbers[0],
                        PartNo = Gen_PartNo(),
                        PartDesc = Gen_PartDesc(),
                        Quantity = 2
                    },
                    new BomFile.BomFileLotPart {
                        LotNo = lotNumbers[1],
                        PartNo = Gen_PartNo(),
                        PartDesc = Gen_PartDesc(),
                        Quantity = 3
                    },
                    new BomFile.BomFileLotPart {
                        LotNo = lotNumbers[2],
                        PartNo = Gen_PartNo(),
                        PartDesc = Gen_PartDesc(),
                        Quantity = 4
                    }
                },
            LotEntries = lotNumbers.Select(lotNo => new BomFile.BomFileLot {
                LotNo = lotNo,
                Kits = Enumerable.Range(1, 6).Select((n, i) => new BomFile.BomFileLot.BomFileKit {
                    KitNo = lotNo + i.ToString().PadLeft(2, '0'),
                    PcvCode = lotNo.Substring(0, EntityFieldLen.Pcv_Code)
                }).ToList()
            }).ToList()

        };

    }
}

