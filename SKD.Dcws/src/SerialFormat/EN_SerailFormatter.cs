
using System.Text.RegularExpressions;

namespace SKD.Dcws {
    public class EN_SerialFormatter {

        public static string INVALID_SERIAL = "Invalid EN serial";
        public SerialFormatResult FormatSerial(string input) {
            var pattern = @"(\w+)(\s+)(\w+)(\s+)(\w+)";
            var regex = new Regex(pattern);

            var matches = regex.Match(input);        
            return new SerialFormatResult(
                matches.Value, 
                Success: matches.Success,
                Message: matches.Success ? "":  INVALID_SERIAL
            );
        }
    }
}