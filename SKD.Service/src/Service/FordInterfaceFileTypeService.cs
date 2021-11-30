
namespace SKD.Service;

public enum FordInterfaceFileType {
    BOM,
    SHIP,
    VIN,
    PARTNER_STATUS_ACK,
    UNKOWN
}

public static class FordInterfaceFileTypeService {

    private static List<(string regex, FordInterfaceFileType fileType)> ImportFileRegexPatterns =
        new List<(string regex, FordInterfaceFileType fileType)> {
                (@"\.BOM", FordInterfaceFileType.BOM),
                (@"\.SHIP", FordInterfaceFileType.SHIP),
                (@"PARTNER_KITPVIN", FordInterfaceFileType.VIN),
                (@"PARTNER_STATUS_ACK", FordInterfaceFileType.PARTNER_STATUS_ACK),
        };

    public static FordInterfaceFileType GetFordInterfaceFileType(string filename) {
        foreach (var importFilePattern in ImportFileRegexPatterns) {
            var regex = new Regex(importFilePattern.regex);
            if (regex.Match(filename).Success) {
                return importFilePattern.fileType;
            }
        }

        return FordInterfaceFileType.UNKOWN;
    }
}
