namespace SKD.Dcws;
public class DcwsSerialFormatter {

    /// <summary> 
    /// Transforms EN or TR serial into format required by the Ford DCWS service
    /// Otherwisre return the serial numbers unchanged
    /// </summary>
    /// <returns>SerialFormatResult:  with Success false if there was an error</returns>
    public static SerialFormatResult FormatSerialIfNeeded(string ComponentTypeCode, Serials serials) {
        switch (ComponentTypeCode) {
            case "EN": {
                    return EN_SerialFormatter.FormatSerial(serials);
                }
            case "TR": {
                    var formatter = new TR_SerialFormatter();
                    return formatter.FormatSerial(serials);
                }
            // return the original serial unchanged
            default: return new SerialFormatResult(serials, true, "");
        }
    }
}
