using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace SKD.Dcws {

    public record TR_SerialVariant(string VariantCode, string InputPattern, string OutputPattern, List<int> Spacing);

    public class TR_SerialFormatter {

        public static int TR_SERIAL_LEN = 39;

        public static string INVALID_SERIAL = "Invalid TR serial";
        public static string UNCHANGED = "";

        private List<TR_SerialVariant> TR_SerailVariants = new List<TR_SerialVariant>{
            new TR_SerialVariant(
                VariantCode: "6R80",
                InputPattern: @"(\w+)(\s+)(\w+)(\s+)(\w+)(\s+)(\w+)(\s+)(\w+)(\s+)(\w+)(\s*)",
                OutputPattern:  @"^(\w+)(\s{1})(\w+)(\s{2})(\w+)(\s{1})(\w+)(\s{1})(\w+)(\s{2})(\w+)(\s{1})$", Spacing: new List<int> { 1, 2, 1, 1, 2, 1 }),
            new TR_SerialVariant(
                VariantCode: "10R80",
                InputPattern: @"(\w{16})(\w{4})\s+(\w{4})\s+(\w{2})\s*",
                OutputPattern: @"",
                Spacing: new List<int> { 6, 1, 1, 5 }),
        };

        public SerialFormatResult FormatSerial(Serials serials) {

            // 
            var serialCombinations = new List<string> {
                serials.Serial1 + serials.Serial2,
                serials.Serial2 + serials.Serial1
            }.Distinct().ToList();

            // get serial and varient
            TR_SerialVariant varient = null;
            var selectedSerial = "";
            foreach (var combination in serialCombinations) {
                varient = Get_TR_SerialVariant(combination);
                if (varient != null) {
                    selectedSerial = combination;
                    break;
                }
            }

            if (varient == null) {
                return new SerialFormatResult(serials, false, INVALID_SERIAL);
            }

            switch (varient.VariantCode) {
                case "6R80": {
                        var newSerial = Format_Variant_6R80(selectedSerial, varient);
                        // verify
                        var matches = Matches(newSerial, varient.OutputPattern);
                        return new SerialFormatResult(new Serials(newSerial, ""), newSerial.Length == TR_SERIAL_LEN, "");
                    }
                case "10R80": {
                        var newSerial = Format_Variant_10R80(selectedSerial, varient);
                        return new SerialFormatResult(new Serials(newSerial, ""), newSerial.Length == TR_SERIAL_LEN, "");
                    }
                default: return new SerialFormatResult(serials, false, INVALID_SERIAL);
            }
        }

        public TR_SerialVariant Get_TR_SerialVariant(string str) {
            foreach (var variant in TR_SerailVariants) {
                if (Matches(str, variant.InputPattern)) {
                    return variant;
                }
            }
            return null;
        }

        private string Format_Variant_6R80(string input, TR_SerialVariant variant) {
            var regex = new Regex(variant.InputPattern);
            var parts = regex.Split(input).Where(t => t.Trim().Length > 0).ToList();

            var newParts = parts.Select((p, i) => p.PadRight(p.Length + variant.Spacing[i], ' ')).ToList();
            var text = String.Join("", newParts);
            return text;
        }

        private string Format_Variant_10R80(string input, TR_SerialVariant variant) {
            var regex = new Regex(variant.InputPattern);
            var parts = regex.Split(input.Trim())
                .Select(t => t.Trim())
                .Where(t => t.Length > 0)
                .ToList();

            var newParts = parts
                .Select((p, i) => p + "".PadRight(variant.Spacing[i], ' '))
                .ToList();

            var text = String.Join("", newParts);
            return text;
        }

        public bool Matches(string Input, string RegexPattern) {
            var regex = new Regex(RegexPattern);
            var result = regex.Match(Input);
            return result.Success;
        }
    };
}

