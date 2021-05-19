
using System.Text.RegularExpressions;

namespace SKD.Dcws {
    public class EN_SerialFormatter {

        public static string  EN_SERIAL_PATTERN = @"(\w+)(\s+)(\w+)(\s+)(\w+)";
        public static string INVALID_SERIAL = "Invalid EN serial";
        public SerialFormatResult FormatSerial(Serials serials) {
            var regex = new Regex(EN_SERIAL_PATTERN);

            var matches = regex.Match(serials.Serial1);        
            return new SerialFormatResult(
                Serials: new Serials(matches.Value, serials.Serial2),
                Success: matches.Success,
                Message: matches.Success ? "":  INVALID_SERIAL
            );
        }
    }
}