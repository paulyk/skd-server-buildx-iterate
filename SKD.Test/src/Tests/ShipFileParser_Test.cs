namespace SKD.Test;

public class ShipFileParser_Test : TestBase {

    private readonly string fileText =
@" HEADERHPUDABRIG2-7F202106151645030157CMMS9CCA
00000101BPA0A02210950011366A1366A     GQQLA04O
00000202SUDU5653774000002000532021-06-15000004082.40
00000303  TKCBN01        1 0155418000939.8*000579.1*000444.5AB39-5418-AA                                                PLT-CAB MTNG FR MBR           000001200008.6184EA
00000403  TKCBN01        0 0155418                          JB3C-5418-CB                                                PLT-CAB MTNG FR MBR           000001200009.0720EA
00000503  TKCBN01        0 0155418                          JB3C-5598-AB                                                SPCR RR SPG                   000000600002.2680EA
00019202TCNU6120457000002000542021-06-15000005126.59
00019303  TKCBN01        1 0155411000939.8*000579.1*000444.5JB3T-14017-LA3JA6                                           SW ASY-DR LK                  000000600000.4536EA
00019403  TKCBN01        0 0155411                          JB3T-14017-MA3JA6                                           SW ASY-DR LK                  000000600000.4536EA
00019503  TKCBN01        0 0155411                          AB39-16164-AA                                               MLDG FRT FNDR FRT(CRASH BLOCK)000000600005.4432EA
00019603  TKCBN01        0 0155411                          AB39-16165-AA                                               MLDG FRT FNDR FRT LH(CRASH BLO000000600005.4432EA
9TRAILRHPUDABRIG2-7F202106151645030000702
";

    [Fact]
    public void Can_parse_ship_file_header() {
        // setup
        var serivce = new ShipFileParser();

        // act 
        var headerLine = fileText.Split('\n').First();
        var (plantCode, sequence, dateCreated) = serivce.ParseHeaderLine(headerLine);

        // assert
        var expectedPlantCode = "HPUDA";
        Assert.Equal(expectedPlantCode, plantCode);

        var exptectedSequence = 157;
        Assert.Equal(exptectedSequence, sequence);

        var expectedDate = new DateTime(2021, 6, 15);
        Assert.Equal(expectedDate, dateCreated);
    }

    [Fact]
    public void Can_parse_ship_file_lot() {
        // setup
        var serivce = new ShipFileParser();

        // act 
        var line = fileText.Split('\n').Skip(1).First();
        var result = serivce.ParseLotLine(line);

        // assert
        var expectedLotNo = "BPA0A0221095001";
        Assert.Equal(expectedLotNo, result.LotNo);
    }

    [Fact]
    public void Can_parse_ship_file_invoice() {
        // setup
        var serivce = new ShipFileParser();

        // act 
        var line = fileText.Split('\n').Skip(2).First();
        var result = serivce.ParseInvoiceLine(line);

        var exptectedInvoiceNo = "00000200053";
        var expectedShiptDate = DateTime.Parse("2021-06-15");
        // assert
        Assert.Equal(exptectedInvoiceNo, result.InvoiceNo);
        Assert.Equal(expectedShiptDate, result.ShipDate);
    }

    [Fact]
    public void Can_parse_ship_file_part() {
        // setup
        var serivce = new ShipFileParser();

        // act 
        var line = fileText.Split('\n').Skip(3).First();
        var result = serivce.ParsePartLine(line);

        var expected_PartNo = "AB39-5418-AA";
        var expteced_HandlingUnitCode = "0155418";
        var expected_Quantity = Int16.Parse("0000012");
        var expected_PartDesc = "PLT-CAB MTNG FR MBR";

        // assert
        Assert.Equal(expected_PartNo, result.PartNo.Trim());
        Assert.Equal(expected_PartDesc, result.CustomerPartDesc.Trim());
        Assert.Equal(expteced_HandlingUnitCode, result.HandlingUnitCode);
        Assert.Equal(expected_Quantity, result.Quantity);
    }

    [Fact]
    public void Can_parse_ship_file() {
        var service = new ShipFileParser();

        // act
        var shipFile = service.ParseShipmentFile(fileText);

        // assert
        var expectedPlantCode = "HPUDA";
        Assert.Equal(expectedPlantCode, shipFile.PlantCode);

        var exptectedSequence = 157;
        Assert.Equal(exptectedSequence, shipFile.Sequence);

        var expectedDate = new DateTime(2021, 6, 15);
        Assert.Equal(expectedDate, shipFile.Created);

        var expectedLotCount = 1;
        var actualLotCount = shipFile.Lots.Count;
        Assert.Equal(expectedLotCount, actualLotCount);

        var exptecedInvoiceCount = 2;
        var actualInvoiceCount = shipFile.Lots.SelectMany(x => x.Invoices).Count();
        Assert.Equal(exptecedInvoiceCount, actualInvoiceCount);

        var exptecedPartCount = 7;
        var actualPartCount = shipFile.Lots.SelectMany(x => x.Invoices).SelectMany(x => x.Parts).Count();
        Assert.Equal(exptecedPartCount, actualPartCount);
    }
}

