using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace SKD.Dcws {

    public record Variant(string VariantCode, string RegexPattern, List<int> spacing, int Length);

    public record FormatResult(string Code, bool Success, bool ValidTRCode);

    public class TR_SerialFormatter {

        private List<Variant> Variants = new List<Variant>{
            new Variant(
                "6R80",
                @"(\w+)(\s+)(\w+)(\s+)(\w+)(\s+)(\w+)(\s+)(\w+)(\s+)(\w+)(\s*)",
                new List<int> { 1, 2, 1, 1, 2, 1 },
                39),
            new Variant(
                "10R80",
                @"(\w{16})(\w{4})\s+(\w{4})\s+(\w{2})\s*",
                new List<int> { 6, 1, 1, 5 },
                39)
        };

        public FormatResult Format_TR_Serial(string input) {
            var varient = GetVariant(input);
            if (varient == null) {
                return new FormatResult("", false, false);
            }

            switch (varient.VariantCode) {
                case "6R80": return Format_Variant_6R80(input, varient);
                case "10R80": return Format_Variant_10R80(input, varient);
            }

            throw new Exception("Bug in Format_TR_Serial");
        }

        public Variant GetVariant(string str) {
            foreach (var variant in Variants) {
                var regex = new Regex(variant.RegexPattern);
                var result = regex.Match(str);
                if (result.Success) {
                    return variant;
                }
            }
            return null;
        }

        private FormatResult Format_Variant_6R80(string input, Variant variant) {
            var regex = new Regex(variant.RegexPattern);
            var parts = regex.Split(input).Where(t => t.Trim().Length > 0).ToList();

            var newParts = parts.Select((p, i) => p.PadRight(p.Length + variant.spacing[i], ' ')).ToList();
            var text = String.Join("", newParts);
            FormatResult result = new FormatResult(text, text.Length == variant.Length, true);
            return result;
        }

        private FormatResult Format_Variant_10R80(string input, Variant variant) {
            var regex = new Regex(variant.RegexPattern);
            var parts = regex.Split(input.Trim())
                .Select(t => t.Trim())
                .Where(t => t.Length > 0)
                .ToList();

            var newParts = parts
                .Select((p, i) => p + "".PadRight(variant.spacing[i], ' '))
                .ToList();

            var text = String.Join("", newParts);
            FormatResult result = new FormatResult(text, text.Length == variant.Length, true);
            return result;            
        }
    };
}

