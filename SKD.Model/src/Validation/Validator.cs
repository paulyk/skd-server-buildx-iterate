using System.Text.RegularExpressions;

namespace SKD.Model {
    public class Validator {
         public bool ValidVIN(string vin) {
            var regex = new Regex(@"[A-Z0-9]{17}");
            var result = regex.Match(vin ?? "");
            return result.Success;
        }
    }
}