namespace SKD.Dcws {
    public class DcwsSerialFormatter {

        public SerialFormatResult FormatSerial(string ComponentTypeCode, string Serial) {
            switch (ComponentTypeCode) {
                case "EN": {
                        var formatter = new EN_SerialFormatter();
                        return formatter.FormatSerial(Serial);
                    }
                case "TR": {
                        var formatter = new TR_SerialFormatter();
                        return formatter.FormatSerial(Serial);
                    }
                // return the original serial unchanged
                default: return new SerialFormatResult(Serial, true, "");
            }
        }
    }
}