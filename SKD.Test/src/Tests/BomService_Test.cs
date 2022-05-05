
namespace SKD.Test;
using PartQuantities = IEnumerable<(string partNo, int quantity)>;

public class BomService_Test : TestBase {

    public BomService_Test() {
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data(generateLot: false);
    }

    [Fact]
    public async Task Can_import_bom_file() {

        // setup
        var plant = Gen_Plant();
        var model = await context.VehicleModels.FirstOrDefaultAsync();
        var lotNo = Gen_LotNo(model.Code, 1);

        var partQuantities = new List<(string partNo, int quantity)>{
                ("part-1", 6),
                ("part-2", 12)
            };

        var input = Gen_BomFileInput(plant.Code, new string[] { lotNo }, 6, partQuantities);

        // test
        var service = new BomService(context);
        var result = await service.ImportBom(input);

        // assert no errors
        var exptectErrorCount = 0;
        var actualErrorCount = result.Errors.Count();
        Assert.Equal(exptectErrorCount, actualErrorCount);

        var bom = await context.Boms
            .Include(t => t.Lots).ThenInclude(t => t.Kits)
            .Include(t => t.Lots).ThenInclude(t => t.LotParts)
            .FirstOrDefaultAsync(t => t.Plant.Code == plant.Code);

        // assert lots and kits
        var exptectedLotCount = input.LotEntries.Count();
        var lotCount = bom.Lots.Count;
        Assert.Equal(exptectedLotCount, lotCount);

        var expectedKitCount = input.LotEntries.SelectMany(t => t.Kits).Count();
        var actualKitCount = bom.Lots.Sum(t => t.Kits.Count);
        Assert.Equal(expectedKitCount, actualKitCount);

        // assert lot parts
        var expectedLotPartCount = input.LotParts.Count();
        var actualLotPartCount = bom.Lots.SelectMany(t => t.LotParts).Count();
        Assert.Equal(expectedLotPartCount, actualLotPartCount);
    }

    [Fact]
    public async Task Import_bom_reformats_part_no() {

        // setup
        var plant = Gen_Plant();
        var lotNumbers = new string[] { Gen_LotNo(1), Gen_LotNo(2) };

        // trim tailing [- ]* and removes spaces
        var part_numbers = new List<(string partNo, string reformattedPartNo)>() {
                ("-W716936-S442", "W716936-S442"),
                ("- W716899-  S900 -", "W716899-S900"),
                ("- EB3B-31010-  AF3ZHE -", "EB3B-31010-AF3ZHE"),
                ("       -  W500301-S437    -   ", "W500301-S437")
            };

        PartQuantities partQuantities = part_numbers.Select(pn => (pn.partNo, 6)).ToList();

        var input = Gen_BomFileInput(plant.Code, lotNumbers, 6, partQuantities);

        // test
        var service = new BomService(context);
        var result = await service.ImportBom(input);

        var partService = new PartService(context);

        // assert 
        foreach (var entry in part_numbers) {
            var part = await context.Parts.FirstOrDefaultAsync(t => t.PartNo == entry.reformattedPartNo);
            var formatted = PartService.ReFormatPartNo(part.PartNo);
            Assert.Equal(formatted, part.PartNo);
        }
    }

    [Fact]
    public async Task Cannot_import_duplicate_bom_lot_parts_in_paylaod() {
        // setup
        var plant = Gen_Plant();
        var model = await context.VehicleModels.FirstOrDefaultAsync();
        var lotNo1 = Gen_LotNo(model.Code, 1);
        var lotNo2 = Gen_LotNo(model.Code, 2);

        PartQuantities partQuantities = new List<(string partNo, int quantity)>{
                ("part-1", 6),
                ("part-2", 12)
            };

        var input = Gen_BomFileInput(plant.Code, new string[] { lotNo1, lotNo2 }, 6, partQuantities);

        // create duplicae by resetting lotNo2 to lotNo1 in lot parts
        input.LotParts = input.LotParts.Select(lp => new BomFile.BomFileLotPart {
            LotNo = lotNo1,
            PartNo = lp.PartNo,
            PartDesc = lp.PartDesc,
            Quantity = lp.Quantity
        }).ToList();

        // test
        var service = new BomService(context);
        var result = await service.ImportBom(input);

        // assert
        var expected_error_message = "duplicate Lot + Part number(s) in payload";
        var actual_error_message = result.Errors.Select(t => t.Message).FirstOrDefault();
        Assert.StartsWith(expected_error_message, actual_error_message);
    }

    [Fact]
    public async Task Cannot_import_if_no_lot_parts() {
        // setup
        var plant = Gen_Plant();
        var model = await context.VehicleModels.FirstOrDefaultAsync();
        var lotNo1 = Gen_LotNo(model.Code, 1);

        PartQuantities partQuantities = new List<(string partNo, int quantity)>{
                ("part-1", 6),
                ("part-2", 12)
            };

        var input = Gen_BomFileInput(plant.Code, new string[] { lotNo1 }, 6, partQuantities);

        // assing empty list to lot parts
        input.LotParts = new List<BomFile.BomFileLotPart>();

        // test
        var service = new BomService(context);
        var result = await service.ImportBom(input);

        // assert
        var errorMessage = result.Errors.Select(t => t.Message).FirstOrDefault();
        var expectedError = "No lot parts found";
        Assert.StartsWith(expectedError, errorMessage);
    }

    [Fact]
    public async Task Cannot_import_bom_lot_kits_if_model_missing() {
        // setup
        var plant = Gen_Plant();
        var modelCode = Gen_VehicleModel_Code();
        var lotNo = Gen_LotNo(modelCode, 1);
        var kitCount = 6;

        PartQuantities partQuantities = new List<(string partNo, int quantity)>{
                ("part-1", 6),
                ("part-2", 12)
            };

        var input = Gen_BomFileInput(plant.Code, new string[] { lotNo }, kitCount, partQuantities);

        // test
        var service = new BomService(context);
        var result = await service.ImportBom(input);

        // assert
        var errorCount = result.Errors.Count;
        Assert.Equal(1, errorCount);

        var errorMessage = result.Errors.Select(t => t.Message).FirstOrDefault();
        var expectedErrorMessage = "model codes not in system";
        Assert.StartsWith(expectedErrorMessage, errorMessage);
    }

    [Fact]
    public async Task Cannot_import_bom_lot_kits_already_imported() {
        // setup
        var plant = Gen_Plant();
        var model = await context.VehicleModels.FirstOrDefaultAsync();
        var lotNo = Gen_LotNo(model.Code, 1);
        var kitCount = 6;

        PartQuantities partQuantities = new List<(string partNo, int quantity)>{
                ("part-1", 6),
                ("part-2", 12)
            };

        var input = Gen_BomFileInput(plant.Code, new string[] { lotNo }, kitCount, partQuantities);

        // test
        var service = new BomService(context);
        var result = await service.ImportBom(input);
        var result_2 = await service.ImportBom(input);

        // assert
        var errorMessage = result_2.Errors.Select(t => t.Message).FirstOrDefault();
        var expectedMessage = "Lots already imported";
        Assert.Equal(expectedMessage, errorMessage);
    }

    [Fact]
    public async Task Can_import_bom_with_previously_imported_lots() {
        // setup
        var plant = Gen_Plant();
        Gen_Model_From_Existing_Component_And_Stations();
        var model_1 = await context.VehicleModels.FirstOrDefaultAsync();
        var model_2 = await context.VehicleModels.Skip(1).FirstOrDefaultAsync();
        var lotNo_1 = Gen_LotNo(model_1.Code, 1);
        var lotNo_2 = Gen_LotNo(model_2.Code, 1);
        var kitCount = 6;

        PartQuantities partQuantities = new List<(string partNo, int quantity)>{
                ("part-1", 6),
                ("part-2", 12)
            };
        var input_1 = Gen_BomFileInput(plant.Code, new string[] { lotNo_1 }, kitCount, partQuantities);
        var input_2 = Gen_BomFileInput(plant.Code, new string[] { lotNo_1, lotNo_2 }, kitCount, partQuantities);

        // test
        var service = new BomService(context);
        
        var result_1 = await service.ImportBom(input_1);    
        var lot_1_lot_parts_count = await context.LotParts.CountAsync(t => t.Lot.LotNo == lotNo_1);
        
        var result_2 = await service.ImportBom(input_2);
        
        var errorCount = result_2.Errors.Count();
        Assert.Equal(0, errorCount);

        // lot parts not added twice
        var lot_1_lot_parts_count_2 = await context.LotParts.CountAsync(t => t.Lot.LotNo == lotNo_1);
        Assert.Equal(lot_1_lot_parts_count, lot_1_lot_parts_count_2);
    }


    [Fact]
    public async Task Can_set_lot_note() {
        Gen_Bom_Lot_and_Kits();
        // setup
        var lot = await context.Lots.FirstOrDefaultAsync();

        var service = new BomService(context);

        //
        var note = "The note";
        var input = new LotNoteInput(lot.LotNo, note);
        var paylaod = await service.SetLotNote(input);

        var updatedLot = await context.Lots.FirstOrDefaultAsync(t => t.LotNo == input.LotNo);

        Assert.Equal(input.Note, updatedLot.Note);
    }

}

