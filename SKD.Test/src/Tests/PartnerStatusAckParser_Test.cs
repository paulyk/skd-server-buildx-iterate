namespace SKD.Test;

public class PartnerStatusAckParser_Test : TestBase {

    private readonly string fileText =
@"HDRPARTNER_STATUS_ACK  HPUDAGQQLA   0002072022-04-01        
DTLACCEPTED  000000031900000003170000000002                 
TLRPARTNER_STATUS_ACK  HPUDAGQQLA0000000003                 
";

    [Fact]
    public void Can_parse_partner_status_ack_file() {
        // setup
        var serivce = new PartnerStatusAckService();

        // act 
        var result = serivce.Parse(fileText);

        // assert
        var expectedPlantCode = "HPUDA";
        Assert.Equal(expectedPlantCode, result.PlantCode);

        var partnerPlantCode = "GQQLA";
        Assert.Equal(partnerPlantCode, result.PartnerPlantCode);

        var sequence = 207;
        Assert.Equal(sequence, result.Sequence);

        var totalProcessed =  319;
        Assert.Equal(totalProcessed, result.TotalProcessed);

        var totalAccepted =  317;
        Assert.Equal(totalAccepted, result.TotalAccepted);

        var totalRejected =  2;
        Assert.Equal(totalRejected, result.TotalRejected);

        var expectedProcessedDate = new DateTime(2022,4,1);
        var actualDate = DateTime.Parse(result.FileDate);
        Assert.Equal(expectedProcessedDate, actualDate);
    }
}

