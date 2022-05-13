namespace SKD.Dcws;
public record Serials(string Serial1, string Serial2);
public record SerialFormatResult(Serials Serials, bool Success, string Message);
