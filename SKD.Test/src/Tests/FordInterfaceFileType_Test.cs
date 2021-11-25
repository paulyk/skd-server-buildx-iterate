namespace SKD.Test;
public class FordInterfaceFileType_Test {

    record TestSet(string filename, FordInterfaceFileType expectedFileType);

    private List<TestSet> testPatterns = new List<TestSet> {
            new TestSet("PARTNER_KITPVIN_HPUDA_GQQLA_211018_103000", FordInterfaceFileType.VIN),
            new TestSet("MMP7.RMA.VWMPE.CMMS3WRA.BRIDG27A.BOM_2021_09_29_0701", FordInterfaceFileType.BOM),
            new TestSet("MMP7.RMA.J61PE.CMMS9CCA.BRIDG27F.SHIP_2021_09_18_0701", FordInterfaceFileType.SHIP),
            new TestSet("PARTNER_STATUS_ACK_HPUDA_GQQLA", FordInterfaceFileType.PARTNER_STATUS_ACK),
            new TestSet("skd-prod-2021-10-08-12/55.bacpac", FordInterfaceFileType.UNKOWN),
            new TestSet("MMP7.RMA.VWMPE.CMMS3WRA.BRIDG27A.bom_2021_09_29_0701", FordInterfaceFileType.UNKOWN),
        };

    [Fact]
    public void Can_detect_file_types_correctly() {

        foreach (var test in testPatterns) {
            var actualFileType = FordInterfaceFileTypeService.GetFordInterfaceFileType(test.filename);
            Assert.Equal(test.expectedFileType, actualFileType);
        }

    }
}
