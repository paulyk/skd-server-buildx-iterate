namespace SKD.Test;
public class FlatFile_Test {

    public class HeaderLayout {
        public readonly int RECORD_TYPE = 3;
        public readonly int SEQUENCE = 4;
        public readonly int NAME = 10;
    }

    [Fact]
    public void FlatFileLine_builds_correct_shema_from_schema_type() {
        var headerLayoutInstance = new HeaderLayout();

        var headerLine = new FlatFileLine<HeaderLayout>();
        Assert.Equal(3, headerLine.Fields.Count);

        Assert.Equal(headerLayoutInstance.RECORD_TYPE, headerLine.Fields.First(t => t.Name == "RECORD_TYPE").Length);
        Assert.Equal(headerLayoutInstance.SEQUENCE, headerLine.Fields.First(t => t.Name == "SEQUENCE").Length);
        Assert.Equal(headerLayoutInstance.NAME, headerLine.Fields.First(t => t.Name == "NAME").Length);
    }

    [Fact]
    public void FlatFileLine_builds_correct_output() {
        var lineBuilder = new FlatFileLine<HeaderLayout>();

        var fieldValues = new List<FlatFileLine<HeaderLayout>.FieldValue> {
                lineBuilder.CreateFieldValue(t => t.RECORD_TYPE, "HDR"),
                lineBuilder.CreateFieldValue(t => t.SEQUENCE,"0004"),
                lineBuilder.CreateFieldValue(t => t.NAME,"1234567890")
            };

        var output = lineBuilder.Build(fieldValues);
        var expected = String.Join("", fieldValues.Select(t => t.Value));

        Assert.Equal(expected, output);
        Assert.Equal(lineBuilder.LineLength, output.Length);
    }

    [Fact]
    public void FlatFileLine_parses_corectly() {

        var lineBuilder = new FlatFileLine<HeaderLayout>();

        var inputFields = new List<FlatFileLine<HeaderLayout>.FieldValue> {
                lineBuilder.CreateFieldValue(t => t.RECORD_TYPE, "HDR"),
                lineBuilder.CreateFieldValue(t => t.SEQUENCE,"0004"),
                lineBuilder.CreateFieldValue(t => t.NAME,"1234567890")
            };
        var expectedText = String.Join("", inputFields.Select(t => t.Value));

        var outputFields = lineBuilder.Parse(expectedText);
        var outputText = String.Join("", outputFields.Select(t => t.Value));

        Assert.Equal(expectedText, outputText);
    }

    [Fact]
    public void FlatFileLine_can_set_field() {
        // setup
        var lineBuilder = new FlatFileLine<HeaderLayout>();

        var recordType = "HDR";
        var sequence = "0004";
        var name = "1234567890";

        var inputFields = new List<FlatFileLine<HeaderLayout>.FieldValue> {
                lineBuilder.CreateFieldValue(t => t.RECORD_TYPE, recordType),
                lineBuilder.CreateFieldValue(t => t.SEQUENCE,sequence),
                lineBuilder.CreateFieldValue(t => t.NAME, name)
            };
        var line = String.Join("", inputFields.Select(t => t.Value));

        // test
        var newSequence = "0005";
        var newLine = lineBuilder.SetFieldValue(line, t => t.SEQUENCE, newSequence);

        // assert
        var fields = typeof(HeaderLayout).GetFields().Select((v, i) => {
            return new {
                Name = v.Name,
                Index = i
            };
        }).ToList();

        var actual = lineBuilder.GetFieldValue(newLine, x => x.RECORD_TYPE);
        Assert.Equal(recordType, actual);

        actual = lineBuilder.GetFieldValue(newLine, x => x.SEQUENCE);
        Assert.Equal(newSequence, actual);

        actual = lineBuilder.GetFieldValue(newLine, x => x.NAME);
        Assert.Equal(name, actual);
    }
}
