namespace SKD.Dcws {

    public class SerialFormatter {

        public string Format_TR_Serial(string input) {
            if (input.Length == 39) {
                return input;
            }
            // take first 16 chars
            // take theRest
            // return first + pad + theResut must be 39 chars
            // A43##[ ]##############[ ][ ]????[ ]7000[ ]??[ ][ ]??[ ]
            // A43## ##############  ???? 7000 ??  ??

            var first16 = input.Substring(0, 16);
            var theRest = input.Substring(16).Trim();

            var sixChars = "".PadRight(6, ' ');

            var result = $"{first16}{sixChars}{theRest}".PadRight(39, ' ');

            return result;
        }


    }
}

/*
function toCDC_2_CompatibleForamt(input) {
   let parts = input.split('-')
   let middle = parts[1].length === 9 ? parts[1].padEnd–––(15, 'x') : parts[1]
   return `${parts[0]}-${middle}-${parts[2]}`
}
*/