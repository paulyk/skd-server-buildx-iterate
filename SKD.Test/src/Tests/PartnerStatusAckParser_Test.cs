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
        var serivce = new PartnerStatusAckParser();

        // act 
        var result = serivce.Parse(fileText);

        // assert
        var expectedPlantCode = "HPUDA";
        Assert.Equal(expectedPlantCode, result.PlantCode);

        var partnerPlantCode = "GQQLA";
        Assert.Equal(partnerPlantCode, result.PartnerPlantCode);

        var sequence = 207;
        Assert.Equal(sequence, result.Sequence);

        var status =  "ACCEPTED  ";
        Assert.Equal(status, result.Status);

        var total =  319;
        Assert.Equal(total, result.Total);

        var accepted =  317;
        Assert.Equal(accepted, result.Accepted);

        var rejected =  2;
        Assert.Equal(rejected, result.Rejected);

    }

}

