using System;
using System.Linq;
using Xunit;
using SKD.Dcws;
using System.Collections.Generic;

namespace SKD.Test {
    public class DcwsService_Test {

        record TestData(string Serial, string ExpectedSerial, bool ExpectedSuccess, string Note, bool ThrowsException);

        [Fact]
        public void dcws_serial_formatter_transformats_tr_serial_correctly() {

            // setup
            var tests = new List<TestData> {
                
                new TestData(
                    Serial:         "A4321 03092018787960  FB3P 7000 DA  A1 ",
                    ExpectedSerial: "A4321 03092018787960  FB3P 7000 DA  A1 ",
                    ExpectedSuccess: true,
                    Note: " { 1, 2, 1, 1, 2, 1 })",
                    ThrowsException: false
                ),

                new TestData(
                    Serial: "A4321 03092018881360 FB3P 7000  DA A1    ",
                    ExpectedSerial: "A4321 03092018881360  FB3P 7000 DA  A1 ",
                    ExpectedSuccess: true,
                    Note: "",
                    ThrowsException: false
                ),
                new TestData(
                    Serial: "TC04A21034221034L1MP 7000 SB ",
                    ExpectedSerial:  "TC04A21034221034      L1MP 7000 SB     ",
                    ExpectedSuccess:true,
                    Note: "",
                    ThrowsException: false
                ),
                new TestData(
                    Serial:         "TC04A21034221034      L1MP 7000 SB     ",
                    ExpectedSerial:   "TC04A21034221034      L1MP 7000 SB     ",
                    ExpectedSuccess:true,
                    Note: "",
                    ThrowsException: false
                ),
                new TestData(
                    Serial:"JB3B-2660004-JH3ZHE",
                    ExpectedSerial: " ",
                    ExpectedSuccess:false,
                    Note: "",
                    ThrowsException: true
                ),
                new TestData(
                    Serial:"P5AT2943775",
                    ExpectedSerial: " ",
                    ExpectedSuccess:false,
                    Note: "",
                    ThrowsException: true
                ),
            };

            // test
            var trFormatter = new TR_SerialFormatter();
            foreach (var testEntry in tests) {

                if (testEntry.ThrowsException) {
                    Assert.Throws<Exception>(() => {
                        var result = trFormatter.Format_TR_Serial(testEntry.Serial);    
                    });
                } else {

                    var result = trFormatter.Format_TR_Serial(testEntry.Serial);

                    // assert
                    Assert.Equal(testEntry.ExpectedSuccess, result.Success);

                    if (testEntry.ExpectedSuccess) {
                        var equal = testEntry.ExpectedSerial == result.Serial;
                        Assert.Equal(testEntry.ExpectedSerial, result.Serial);
                    }
                }
            }
        }
    }
}