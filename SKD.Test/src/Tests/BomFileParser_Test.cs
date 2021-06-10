using System;
using System.Collections.Generic;
using SKD.Service;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using SKD.Dcws;

namespace SKD.Test {
    public class BomFileParser_Test : TestBase {

        private string BomFileText =
@" HEADERHPUDABRIG2-7A20201222123231WSPA91366A0063CMMS3AMA
DTLBPA0A0F21028001012021-01-28200WSPA9TKDCATCH PART            CAT   YL84- 58024A38-HAA     -                                     0000000003.00000
DTLBPA0A0F21028001012021-01-28200WSPA9TKDCABIN01               KIT   AB39-    5A216-AB      -   PIP-MFLR INLT INTERM              0000000001.00000
DTLBPA0A0F21028001012021-01-28200WSPA9TKDCABIN01               KIT   AB39-    5K214-BF      -   MFLR & OLET PIP ASY               0000000001.00000
DTLBPA0A0F21028001022021-01-28200WSPA9TKDCABIN01               KIT   EB3B-    16450-CK5GAX  -   BD ASY RNG                        0000000001.00000
DTLBPA0A0F21028001022021-01-28200WSPA9TKDCABIN01               KIT   EB3B-    16451-CK5GAX  -   BD ASY RNG LH                     0000000001.00000";


        [Fact]
        public void can_parse_bom_file_header() {
            // setup
            var parser = new BomFileParser(BomFileText);

            // act
            var headerValues = parser.ParseHeaderLine();

            // assert
            var plantCode = headerValues.First(t => t.Name == nameof(BomFileLayout.Header.HDR_KD_PLANT_GSDB)).Value;
            Assert.Equal("HPUDA", plantCode);

            var seqNumStr = headerValues.First(t => t.Name == nameof(BomFileLayout.Header.HDR_BRIDGE_SEQ_NBR)).Value;
            Assert.Equal("0063", seqNumStr);

        }

        [Fact]
        public void can_parse_bom_file_detail() {
            // setup
            var parser = new BomFileParser(BomFileText);

            // act
            var detailLineValues = parser.ParseDetailLines();
            var lineValues = detailLineValues[0];

            // assert
            var lineType = lineValues.First(t => t.Name == nameof(BomFileLayout.Detail.KBM_RECORD_TYPE)).Value;
            Assert.Equal("DTL", lineType);

            var expectedLotNo = "BPA0A0F21028001";
            var actualLotNo = lineValues.First(t => t.Name == nameof(BomFileLayout.Detail.KBM_LOT_NUMBER)).Value;
            Assert.Equal(expectedLotNo, actualLotNo);
        }

        [Fact]
        public void can_build_bom_lot_kit_input() {
            var parser = new BomFileParser(BomFileText);
            var input = parser.BuildBomLotKitInput();

            var lotCount = input.Lots.Count();
            Assert.Equal(1, lotCount);
            
            var kitCount = input.Lots.SelectMany(t => t.Kits).Count();
            Assert.Equal(2, kitCount);
        }
    }
}