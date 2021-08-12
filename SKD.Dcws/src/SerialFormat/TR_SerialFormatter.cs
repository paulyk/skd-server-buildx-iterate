using System;
using System.Collections.Generic;
using System.Linq;

namespace SKD.Dcws {

    public record MatchVarientResult(TR_Varient Varient, Serials Serials);
    public enum TR_Variant_Type {
        V_6R80,
        V_10R80,
        V_MT
    }

    public class TR_Varient {
        public TR_Variant_Type VarientType { get; set; }
        public string Match_Serial1_Regex { get; set; } = "";
        public string Match_Serial_2_Regex { get; set; } = "";
        public string Tokenize_Regex { get; set; } = "";
        public string Math_Ouput_Regex { get; set; } = "";
        public List<int> TokenSpacing { get; set; } = new List<int>();
    }

    public class TR_SerialFormatter {

        readonly SerialUtil serialUtil = new();

        public static readonly string NO_MATCHIN_TR_VARIENT = "No matching TR variant";

        public List<TR_Varient> TR_Varients = new () {
            new TR_Varient {
                VarientType = TR_Variant_Type.V_6R80,
                Match_Serial1_Regex = @"^(\w+)\s+(\w+)\s+(\w+)\s+(\w+)\s+(\w+)\s+(\w+)\s+$",
                Match_Serial_2_Regex = @"\s*",                
                Math_Ouput_Regex = @"^\w+\s{1}\w+\s{2}\w+\s{1}\w+\s{1}\w+\s{2}\w+\s$",
                Tokenize_Regex = @"^(\w+)\s+(\w+)\s+(\w+)\s+(\w+)\s+(\w+)\s+(\w+)\s+$",
                TokenSpacing = new List<int> { 1, 2, 1, 1, 2, 1 }
            },
            new TR_Varient {
                VarientType = TR_Variant_Type.V_10R80,
                Match_Serial1_Regex = @"^(\w{16})(\w{4})\s+(\w{4})\s+(\w{2})\s*$",
                Match_Serial_2_Regex = @"\s*",
                Math_Ouput_Regex = @"^\w{16}\s{6}\w{4}\s\w{4}\s\w{2}\s{5}",
                Tokenize_Regex = @"^(\w{16})(\w{4})\s+(\w{4})\s+(\w{2})\s*$",

                TokenSpacing = new List<int> { 6, 1, 1, 5 }
            },
            new TR_Varient {
                VarientType = TR_Variant_Type.V_MT,
                Match_Serial1_Regex = @"FFTB\w{12}",
                Match_Serial_2_Regex = @"\w{4}\s7003\s\w{2}",
                Math_Ouput_Regex = @"^FFTB\d{12}\w{4}\s7003\s\w{2}$",
                Tokenize_Regex = @"(\w+)\s(\w+)\s(\w+)",                
                TokenSpacing = new List<int> { 1, 1 }
            }
        };

        public SerialFormatResult FormatSerial(Serials inputSerials) {
            var (Varient, Serials) = Get_TR_Variant(inputSerials);
            if (Varient == null) {
                return new SerialFormatResult(inputSerials, false, NO_MATCHIN_TR_VARIENT);
            }

            var serial = Serials.Serial1 + Serials.Serial2;
            var formattedSerial = serialUtil.SpacifyString(serial, Varient.Tokenize_Regex, Varient.TokenSpacing);

            var matchesOutputFormat = serialUtil.MatchesPattern(formattedSerial, Varient.Math_Ouput_Regex);

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
            var serials_combinations = new List<Serials> {
                new Serials(serials.Serial1, serials.Serial2),
                new Serials(serials.Serial2, serials.Serial1),
            };


            foreach (var trVariant in TR_Varients) {
                foreach (var serialsCombo in serials_combinations) {
                    var serial_1_match = serialUtil.MatchesPattern(serialsCombo.Serial1, trVariant.Match_Serial1_Regex);
                    var serial_2_match = serialUtil.MatchesPattern(serialsCombo.Serial2, trVariant.Match_Serial_2_Regex);

                    if (serial_1_match && serial_2_match) {
                        return new MatchVarientResult(trVariant, serialsCombo);
                    }
                }
            }
            return new MatchVarientResult(null, serials);
        }
    }
}

