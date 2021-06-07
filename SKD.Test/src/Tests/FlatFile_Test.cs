using System;
using System.Collections.Generic;
using SKD.Model;
using SKD.Service;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using SKD.Service.Util;

namespace SKD.Test {
    public class FlatFile_Test {

        public class HeaderLayout {
            public static int RECORD_TYPE = 3;
            public static int SEQUENCE = 4;
            public static int NAME = 10;
        }

        [Fact]
        public void FlatFileLine_builds_correct_shema_from_schema_type() {

            var headerLine = new FlatFileLine(typeof(HeaderLayout));
            Assert.Equal(3, headerLine.Fields.Count);

            Assert.Equal(HeaderLayout.RECORD_TYPE, headerLine.Fields.First(t => t.Name == "RECORD_TYPE").Length);
            Assert.Equal(HeaderLayout.SEQUENCE, headerLine.Fields.First(t => t.Name == "SEQUENCE").Length);
            Assert.Equal(HeaderLayout.NAME, headerLine.Fields.First(t => t.Name == "NAME").Length);
        }

        [Fact]
        public void FlatFileLine_builds_correct_output() {

            var headerLine = new FlatFileLine(typeof(HeaderLayout));

            var fieldValues = new List<FlatFileLine.FieldValue> {
                new FlatFileLine.FieldValue {
                    Name = "RECORD_TYPE",
                    Value = "HDR"
                },
                new FlatFileLine.FieldValue {
                    Name = "SEQUENCE",
                    Value = "0004"
                },
                new FlatFileLine.FieldValue {
                    Name = "NAME",
                    Value = "1234567890"
                },
            };

            var output = headerLine.Build(fieldValues);
            var expected = String.Join("", fieldValues.Select(t => t.Value));

            Assert.Equal(expected, output);
            Assert.Equal(headerLine.LineLength, output.Length);
        }

        [Fact]
        public void FlatFileLine_parses_corectly() {
            var headerLine = new FlatFileLine(typeof(HeaderLayout));

            var inputFields = new List<FlatFileLine.FieldValue> {
                new FlatFileLine.FieldValue {
                    Name = "RECORD_TYPE",
                    Value = "HDR"
                },
                new FlatFileLine.FieldValue {
                    Name = "SEQUENCE",
                    Value = "0004"
                },
                new FlatFileLine.FieldValue {
                    Name = "NAME",
                    Value = "1234567890"
                },
            };
            var expectedText = String.Join("", inputFields.Select(t => t.Value));

            var outputFiels = headerLine.Parse(expectedText);
            var outputText = String.Join("", outputFiels.Select(t => t.Value));
            
            Assert.Equal(expectedText, outputText);
         }

    }
}