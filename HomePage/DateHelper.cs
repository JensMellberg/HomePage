namespace HomePage
{
    public static class DateHelper
    {
        public static DateTime DateTimeNow => DateTime.Now;

        public static DateTime DateNow => DateTimeNow.Date;

        public static DateTime AdjustedDateNow
        {
            get
            {
                var datenow = DateTimeNow;
                if (datenow.Hour < 4)
                {
                    return datenow.AddDays(-1);
                }

                return datenow;
            }
        }

        public static string FormatDateForQueryString(DateTime date) => $"year={date.Year}&month={date.Month}&day={date.Day}";

        public static string ToKey(DateTime date) => $"{date.Year}-{date.Month}-{date.Day}";

        public static DateTime FromKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return DateTime.MinValue;
            }

            var tokens = key.Split('-').Select(int.Parse).ToArray();
            return new DateTime(tokens[0], tokens[1], tokens[2]);
        }

        public static string ToNumberedDateString(DateTime date) => $"{date.Day}/{date.Month} {date.Year}";

        public static string KeyWithZerosFromKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            var tokens = key.Split('-');
            if (tokens[1].Length == 1)
            {
                tokens[1] = "0" + tokens[1];
            }

            if (tokens[2].Length == 1)
            {
                tokens[2] = "0" + tokens[2];
            }

            return string.Join('-', tokens);
        }

        public static string KeyFromKeyWithZeros(string key)
        {
            var tokens = key.Split('-');
            if (tokens[1][0] == '0')
            {
                tokens[1] = tokens[1][1].ToString();
            }

            if (tokens[2][0] == '0')
            {
                tokens[2] = tokens[2][1].ToString();
            }

            return string.Join('-', tokens);
        }

        public static DateTime GetFirstOfWeek(DateTime date)
        {
            var returnDate = date;
            while (returnDate.DayOfWeek != DayOfWeek.Monday)
            {
                returnDate = returnDate.AddDays(-1);
            }

            return returnDate;
        }

        public static readonly Dictionary<int, string> MonthNumberToString = new Dictionary<int, string>() {
            {1, "Januari" },
            {2, "Februari" },
            {3, "Mars" },
            {4, "April" },
            {5, "Maj" },
            {6, "Juni" },
            {7, "Juli" },
            {8, "Augusti" },
            {9, "September" },
            {10, "Oktober" },
            {11, "November" },
            {12, "December" }
        };

        public static readonly Dictionary<int, string> WeekNumberToString = new Dictionary<int, string>() {
            {1, "Måndag" },
            {2, "Tisdag" },
            {3, "Onsdag" },
            {4, "Torsdag" },
            {5, "Fredag" },
            {6, "Lördag" },
            {0, "Söndag" }
        };
    }
}
