namespace SKD.Test;

public class Util {
    public static string RandomString(int len) {
        var str = Guid.NewGuid().ToString().Replace("-", "");
        while (len > str.Length) {
            str += str;
        }
        return str.Substring(0, len);
    }
}
