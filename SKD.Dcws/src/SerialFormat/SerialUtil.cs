using System;
using System.Text.RegularExpressions;
using System.Text;

namespace SKD.Dcws {
    public class SerialUtil {
        public List<string> GetWordsFromString(string input) {
            Regex rg = new(@"\w+");
            MatchCollection matches = rg.Matches(input);
            return matches.Select(t => t.Value).ToList();
        }

        public Boolean MatchesPattern(string input, string pattern) {
            Regex rg = new(pattern);
            Match match = rg.Match(input);
            return match.Success;
        }

        public string SpacifyString(string input, string regexPattern, List<int> spacing) {
            Regex regex = new(regexPattern);
            var parts = regex.Match(input).Groups.Values.Skip(1).ToList();

            var builder = new StringBuilder();
            var len = parts.Count > spacing.Count ? parts.Count : spacing.Count;

            for (var i = 0; i < len; i++) {
                if (i < parts.Count) {
                    builder.Append(parts[i]);
                }
                if (i < spacing.Count) {
                    builder.Append(new String(' ', spacing[i]));
                }
            }
            return builder.ToString();
        }
    }
}

