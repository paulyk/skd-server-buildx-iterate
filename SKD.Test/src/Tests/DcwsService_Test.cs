using System;
using System.Linq;
using Xunit;
using SKD.Dcws;
using System.Collections.Generic;

namespace SKD.Test {
    public class DcwsService_Test {

        [Fact]
        public void dcws_serial_formatter_transformats_tr_serial_correctly() {
            // take first 16 chars
            // take theRest
            // first + pad + theResut must be 39 chars

            var tests = new List<(string input, string expected)> {
                ("TC04A20023121484      JB3P 7000 VE     ","TC04A20023121484      JB3P 7000 VE     "),
                ("TC04A20023121484JB3P      7000 VE  ", "TC04A20023121484    JB3P      7000 VE  ")
            };

            var serialFormatter = new SerialFormatter();

            // test 
            tests.ForEach(test => {
                var result = serialFormatter.Format_TR_Serial(test.input);
                Assert.Equal(test.expected, result);
                Assert.Equal(39, result.Length);
            });




        }
    }
}