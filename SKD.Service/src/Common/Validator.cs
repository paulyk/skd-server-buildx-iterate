using System.Text.RegularExpressions;

namespace SKD.Common  {
    public class Validator {

         public bool Valid_VIN(string vin) {
            var regex = new Regex(@"[A-Z0-9]{17}");
            var result = regex.Match(vin ?? "");
            return result.Success;
        }

         public bool Valid_KitNo(string kitNo) {
            var regex = new Regex(@"[A-Z0-9]{17}");
            var result = regex.Match(kitNo ?? "");
            return result.Success;
        }

         public bool Valid_LotNo(string lotNo) {
            var regex = new Regex(@"[A-Z0-9]{15}");
            var result = regex.Match(lotNo ?? "");
            return result.Success;
        }
         public bool Valid_PCV(string pcv) {
            var regex = new Regex(@"^\w{7,11}$");
            var result = regex.Match(pcv ?? "");
            return result.Success;
        }

    }
}