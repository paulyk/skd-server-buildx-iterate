namespace SKD.Test;
public class VinFileParser_Test : TestBase {

    private readonly string fileText =
@"HDRKIT_VIN_MAP         HPUDAGQQLA   0000062021-08-04                                                
DTLBPA0A0221152001BPA0A022115200101MS5LKNF80MM0000432021-08-09                                      
DTLBPA0A0221152001BPA0A022115200102MS5LKNF80MM0000442021-08-09                                      
DTLBPA0A0221152001BPA0A022115200103MS5LKNF80MM0000452021-08-09                                      
DTLBPA0A0221152001BPA0A022115200104MS5LKNF80MM0000462021-08-09                                      
DTLBPA0A0221152001BPA0A022115200105MS5LKNF80MM0000472021-08-09                                      
DTLBPA0A0221152001BPA0A022115200106MS5LKNF80MM0000482021-08-09                                      
TLRKIT_VIN_MAP         HPUDAGQQLA0000000008                                                         
";


    [Fact]
    public void Can_parse_kitvin_file_header() {
        // setup
        var serivce = new VinFileParser();

        // act 
        var headerLine = fileText.Split('\n').First();
        var (plantCode, partnerPlantCode, sequence) = serivce.ParseHeaderLine(headerLine);

        // assert
        var exptectedPlant = "HPUDA";
        Assert.Equal(exptectedPlant, plantCode);

        var exptectedPartnerPlant = "GQQLA";
        Assert.Equal(exptectedPartnerPlant, partnerPlantCode);

        var exptectedSequence = 6;
        Assert.Equal(exptectedSequence, sequence);
    }

    [Fact]
    public void Can_parse_kitvin_detail() {
        // setup
        var serivce = new VinFileParser();

        // act 
        var line = fileText.Split('\n').Skip(1).First();
        var result = serivce.ParseDetailLine(line);

        // assert
        var expectedLotNo = "BPA0A0221152001";
        Assert.Equal(expectedLotNo, result.LotNo);

        var expectedKitNo = "BPA0A022115200101";
        Assert.Equal(expectedKitNo, result.KitNo);

        var expectedVIN = "MS5LKNF80MM000043";
        Assert.Equal(expectedVIN, result.VIN);

    }

    [Fact]
    public void Can_parse_vin_file() {
        var service = new VinFileParser();

        // act
        var kitVinFile = service.ParseVinFile(fileText);

        // assert
        var exptectedPlant = "HPUDA";
        Assert.Equal(exptectedPlant, kitVinFile.PlantCode);

        var exptectedPartnerPlant = "GQQLA";
        Assert.Equal(exptectedPartnerPlant, kitVinFile.PartnerPlantCode);

        var exptectedSequence = 6;
        Assert.Equal(exptectedSequence, kitVinFile.Sequence);

        var exptectedKitCount = 6;
        Assert.Equal(exptectedKitCount, kitVinFile.Kits.Count);

        var expectedLotNo = "BPA0A0221152001";

        var vinBase = "MS5LKNF80MM0000";
        var vinSuffix = 43;
        var kitSuffix = 1;
        foreach (var kit in kitVinFile.Kits) {
            Assert.Equal(expectedLotNo, kit.LotNo);

            var exptectedKitNo = $"{expectedLotNo}{kitSuffix++.ToString().PadLeft(2, '0')}";
            Assert.Equal(exptectedKitNo, kit.KitNo);

            var expectedVIN = $"{vinBase}{vinSuffix++}";
            Assert.Equal(expectedVIN, kit.VIN);
        }
    }
}

