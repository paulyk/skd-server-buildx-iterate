using System;
using System.Linq;

namespace SKD.Seed {
    public static class Util {

        public static DateTime RandomDateTime(DateTime seedDate) {
            var daysInMonth = DateTime.DaysInMonth(seedDate.Year, seedDate.Month);
            var firstDayDate = new DateTime(seedDate.Year, seedDate.Month, 1);
            var r = new Random();
            var days = r.Next(0, daysInMonth - 1);
            Tuple<int, int> time = RandomHourMinute();
            return firstDayDate.AddDays(days).AddHours(time.Item1).AddMinutes(time.Item2);
        }

        static Tuple<int, int> RandomHourMinute() {
            var r = new Random();
            var hour = r.Next(0, 23);
            var minute = r.Next(0, 59);
            return new Tuple<int, int>(hour, minute);
        }

        public static string RandomString(int len) {
            var str = Guid.NewGuid().ToString().Replace("-", "");
            while (len > str.Length) {
                str += str;
            }
            return str.Substring(0, len);
        }
    }
}