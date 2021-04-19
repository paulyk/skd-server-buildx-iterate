using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace SKD.Dcws {

    public record Variant(string VariantCode, string InputPattern, string OutputPattern, List<int> Spacing);

    public record FormatResult(string Serial, bool Success);

    public class TR_SerialFormatter {

        public static int TR_SERIAL_LEN = 39;

        private List<Variant> Variants = new List<Variant>{
            new Variant(
                VariantCode: "6R80",
                InputPattern: @"(\w+)(\s+)(\w+)(\s+)(\w+)(\s+)(\w+)(\s+)(\w+)(\s+)(\w+)(\s*)",
                OutputPattern:  @"^(\w+)(\s{1})(\w+)(\s{2})(\w+)(\s{1})(\w+)(\s{1})(\w+)(\s{2})(\w+)(\s{1})$", Spacing: new List<int> { 1, 2, 1, 1, 2, 1 }),
            new Variant(
                VariantCode: "10R80",
                InputPattern: @"(\w{16})(\w{4})\s+(\w{4})\s+(\w{2})\s*",
                OutputPattern: @"",
                Spacing: new List<int> { 6, 1, 1, 5 }),
        };

        public FormatResult Format_TR_Serial(string serial) {
            if (serial.Length == TR_SERIAL_LEN) {
                return new FormatResult(serial, true);
            }

            var varient = GetVariant(serial);
            if (varient == null) {
                throw new Exception("Unknown TR variant");
            }


            switch (varient.VariantCode) {
                case "6R80": {
                        var newSerial = Format_Variant_6R80(serial, varient);
                        // verify
                        var matches = Matches(newSerial, varient.OutputPattern);
                        if (!matches) {
                            Console.WriteLine("does not match");
                        }

                        return new FormatResult(newSerial, newSerial.Length == TR_SERIAL_LEN);
                    }
                case "10R80": {
                        var newSerial = Format_Variant_10R80(serial, varient);
                        return new FormatResult(newSerial, newSerial.Length == TR_SERIAL_LEN);
                    }
            }

            throw new Exception("Unknown TR variant");
        }

        public Variant GetVariant(string str) {
            foreach (var variant in Variants) {
                if (Matches(str, variant.InputPattern)) {
                    return variant;
                }
            }
            return null;
        }

        private string Format_Variant_6R80(string input, Variant variant) {
            var regex = new Regex(variant.InputPattern);
            var parts = regex.Split(input).Where(t => t.Trim().Length > 0).ToList();

            var newParts = parts.Select((p, i) => p.PadRight(p.Length + variant.Spacing[i], ' ')).ToList();
            var text = String.Join("", newParts);
            return text;
        }

        private string Format_Variant_10R80(string input, Variant variant) {
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

