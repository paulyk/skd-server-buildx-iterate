namespace SKD.Common;
public class Validator {

    public static bool Valid_VIN(string vin) {
        var regex = new Regex(@"[A-Z0-9]{17}");
        var result = regex.Match(vin ?? "");
        return result.Success;
    }

    public static bool Valid_KitNo(string kitNo) {
        var regex = new Regex(@"[A-Z0-9]{17}");
        var result = regex.Match(kitNo ?? "");
        return result.Success;
    }

    public static bool Valid_LotNo(string lotNo) {
        var regex = new Regex(@"[A-Z0-9]{15}");
        var result = regex.Match(lotNo ?? "");
        return result.Success;
    }
    public static bool Valid_PCV(string pcv) {
        var regex = new Regex(@"^\w{7,11}$");
        var result = regex.Match(pcv ?? "");
        return result.Success;
    }
}
