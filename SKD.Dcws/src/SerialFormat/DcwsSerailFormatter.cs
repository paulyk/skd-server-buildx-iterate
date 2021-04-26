namespace SKD.Dcws {
    public class DcwsSerialFormatter {

        
        /// <summary> 
        /// Transforms EN or TR serial into format required by the Ford DCWS service
        /// Other component types will return the original serial unchanged.
        /// </summary>
         /// <returns>SerialFormatResult:  with Success false if there was an error</returns>
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