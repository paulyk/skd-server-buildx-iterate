using System;
using System.Collections.Generic;
using SKD.Service;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using SKD.Dcws;
using SKD.Common;

namespace SKD.Test {
    public class BomFileParser_Test : TestBase {

        private string BomFileText =
@" HEADERHPUDABRIG2-7A20201222123231WSPA91366A0063CMMS3AMA
DTLBPA0A0F21028001012021-01-28200WSPA9TKDCATCH PART            CAT   YL84- 58024A38-HAA     -                                     0000000003.00000
DTLBPA0A0F21028001012021-01-28200WSPA9TKDCABIN01               KIT   AB39-    5A216-AB      -   PIP-MFLR INLT INTERM              0000000001.00000
DTLBPA0A0F21028001012021-01-28200WSPA9TKDCABIN01               KIT   AB39-    5K214-BF      -   MFLR & OLET PIP ASY               0000000001.00000
DTLBPA0A0F21028001022021-01-28200WSPA9TKDCABIN01               KIT   EB3B-    16450-CK5GAZ  -   BD ASY RNG                        0000000001.00000
DTLBPA0A0F21028001022021-01-28200WSPA9TKDCABIN01               KIT   EB3B-    16451-CK5GAX  -   BD ASY RNG LH                     0000000002.00000
9TRAILRHPUDABRIG2-7A20210512123438WSPA91366A0000004824
";



        [Fact]
        public void can_parse_bom_file_header() {
            // setup
            var lineBuilder = new FlatFileLine<BomFileLayout.Header>();
            var headerLineText = BomFileText.Split('\n').First();

            // act 
            var plantCode = lineBuilder.GetFieldValue(headerLineText, t => t.HDR_KD_PLANT_GSDB);
            var seqNumStr = lineBuilder.GetFieldValue(headerLineText, t => t.HDR_BRIDGE_SEQ_NBR);

            // assert
            Assert.Equal("HPUDA", plantCode);
            Assert.Equal("0063", seqNumStr);
        }

        [Fact]
        public void can_build_bom_lot_kit_input() {
            var parser = new BomFileParser();
            var payload = parser.BuildBomLotKitInput(BomFileText);

            var lotCount = payload.Payload.Lots.Count();
            Assert.Equal(1, lotCount);

            var kitCount = payload.Payload.Lots.SelectMany(t => t.Kits).Count();
            Assert.Equal(2, kitCount);
        }

        [Fact]
        public void can_build_bom_lot_part_input() {
            var parser = new BomFileParser();
            var payload = parser.BuildBomLotPartInput(BomFileText);

            var lotCount = payload.Payload.LotParts.Count();
            Assert.Equal(4, lotCount);
        }
    }
}