using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace SKD.Dcws {

    public record MatchVarientResult(TR_Varient Varient, Serials serials);
    public enum TR_Variant_Type {
        V_6R80,
        V_10R80
    }

    public class TR_Varient {
        public TR_Variant_Type VarientType { get; set; }
        public string InputRegexPattern { get; set; }
        public string OutputRegexPattern { get; set; }
        public List<int> TokenSpacing { get; set; } = new List<int>();
    }

    public class TR_SerialFormatter {

        SerialUtil serialUtil = new SerialUtil();

        public static string NO_MATCHIN_TR_VARIENT = "No matching TR variant";

        public List<TR_Varient> TR_Varients = new List<TR_Varient> {
            new TR_Varient {
                VarientType = TR_Variant_Type.V_6R80,
                InputRegexPattern = @"^(\w+)\s+(\w+)\s+(\w+)\s+(\w+)\s+(\w+)\s+(\w+)\s+$",
                OutputRegexPattern = @"^\w+\s{1}\w+\s{2}\w+\s{1}\w+\s{1}\w+\s{2}\w+\s$",
                TokenSpacing = new List<int> { 1, 2, 1, 1, 2, 1 }
            },
            new TR_Varient {
                VarientType = TR_Variant_Type.V_10R80,
                InputRegexPattern = @"^(\w{16})(\w{4})\s+(\w{4})\s+(\w{2})\s*$",
                OutputRegexPattern = @"^\w{16}\s{6}\w{4}\s\w{4}\s\w{2}\s{5}",
                TokenSpacing = new List<int> { 6, 1, 1, 5 }
            }
        };

        public SerialFormatResult FormatSerial(Serials inputSerials) {
            var (Varient, Serials) = Get_TR_Variant(inputSerials);
            if (Varient == null) {
                return new SerialFormatResult(inputSerials, false, NO_MATCHIN_TR_VARIENT);
            }

            var serial = Serials.Serial1 + Serials.Serial2;
            var formattedSerial = serialUtil.SpacifyString(serial, Varient.InputRegexPattern, Varient.TokenSpacing);

            var matchesOutputFormat = serialUtil.MatchesPattern(formattedSerial, Varient.OutputRegexPattern);

            if (!matchesOutputFormat) {
                throw new Exception("Did not match outptut format");
            }


            return new SerialFormatResult(new Serials(formattedSerial, ""), true, "");
        }

        /// <summary>
        /// Tries to find matching TR variant by testing combinations of Serial1 and Serial2
        ///</summary>
        ///<returns>THe varient and serials in the accepted order</returns>
        public MatchVarientResult Get_TR_Variant(Serials serials) {

            // Serail / Part numbers can be scanned in any order
            // Test both to find the correct varient
            var serialCombinations = new List<Serials> {
                new Serials(serials.Serial1, serials.Serial2),
                new Serials(serials.Serial2, serials.Serial1),
            }.Distinct().ToList();

            foreach (var trVariant in TR_Varients) {

                foreach (var serialsEntry in serialCombinations) {
                    var serial = serialsEntry.Serial1 + serialsEntry.Serial2;
                    var matches = serialUtil.MatchesPattern(serial, trVariant.InputRegexPattern);
                    if (matches) {
                        return new MatchVarientResult(trVariant, serialsEntry);
                    }
                }
            }
            return new MatchVarientResult(null, serials);
        }


    }
}

