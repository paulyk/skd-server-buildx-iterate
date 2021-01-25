namespace SKD.Dcws {

    public class SerialFormatter {

        public string Format_TR_Serial(string input) {
            // take first 16 chars
            // take theRest
            // return first + pad + theResut must be 39 chars

            var first16 = input.Substring(0, 16);
            var theRest = input.Substring(16);

            var padChars = "".PadLeft(39 - (first16.Length + theRest.Length), ' ');

            return $"{first16}{padChars}{theRest}";
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