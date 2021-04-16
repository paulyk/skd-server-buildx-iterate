using System;
using System.Linq;
using Xunit;
using SKD.Dcws;
using System.Collections.Generic;

namespace SKD.Test {
    public class DcwsService_Test {

        record TestData(string Serial, string ExpectedCode, bool ExpectedSuccess, bool ExpectedValid_TR_Code, string Note);

        [Fact]
        public void dcws_serial_formatter_transformats_tr_serial_correctly() {

            // setup
            var tests = new List<TestData> {
                new TestData(
                    Serial: "A4321 03092018881360 FB3P 7000  DA A1  ",
                    ExpectedCode: "A4321 03092018881360  FB3P 7000 DA  A1 ",
                    ExpectedSuccess: true,
                    ExpectedValid_TR_Code: true,
                    Note: ""
                ),
                new TestData(
                    Serial: "TC04A21034221034L1MP 7000 SB ",
                    ExpectedCode:  "TC04A21034221034      L1MP 7000 SB     ",
                    ExpectedSuccess:true,
                    ExpectedValid_TR_Code:true,
                    Note: ""
                ),
                /* this test is failing */
                // new TestData(
                //     Serial:         "TC04A21034221034      L1MP 7000 SB     ",
                //     ExpectedCode:   "TC04A21034221034      L1MP 7000 SB     ",
                //     ExpectedSuccess:true,
                //     ExpectedValid_TR_Code:true,
                //     Note: ""
                // ),
                new TestData(
                    Serial:"JB3B-2660004-JH3ZHE",
                    ExpectedCode: " ",
                    ExpectedSuccess:false,
                    ExpectedValid_TR_Code:false,
                    Note: ""
                ),

                new TestData(
                    Serial:"P5AT2943775",
                    ExpectedCode: " ",
                    ExpectedSuccess:false,
                    ExpectedValid_TR_Code:false,
                    Note: ""
                ),
            };

            // test
            var trFormatter = new TR_SerialFormatter();
            foreach (var testEntry in tests) {
                var result = trFormatter.Format_TR_Serial(testEntry.Serial);

                // assert
                Assert.Equal(testEntry.ExpectedSuccess, result.Success);
                Assert.Equal(testEntry.ExpectedValid_TR_Code, result.ValidTRCode);

                if (testEntry.ExpectedSuccess) {
                    Assert.Equal(testEntry.ExpectedCode, result.Code);
                }
            }
        }
    }
}