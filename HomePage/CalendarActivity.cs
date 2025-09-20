using System.Text;

namespace HomePage
{
    public class CalendarActivity : SaveableItem
    {
        public string Key { get; set; } = Guid.NewGuid().ToString();

        [SaveProperty]
        public string Text { get; set; }

        [SaveProperty]
        public string Person { get; set; }

        [SaveProperty]
        public string Date { get; set; }

        [SaveProperty]
        public int DurationInDays { get; set; } = 1;

        [SaveProperty]
        public bool IsReoccuring { get; set; }

        [SaveProperty]
        public bool IsVacation { get; set; }

        public DateTime? DateInCalendar { get; set; }

        public string ReplacedText(int yearNow)
        {
            var index = Text.IndexOf('[');
            var sb = new StringBuilder();
            var result = Text;
            if (index != -1)
            {
                index++;
                while (Text[index] != ']')
                {
                    sb.Append(Text[index]);
                    index++;
                }
            }

            var text = sb.ToString();
            if (!string.IsNullOrEmpty(text))
            {
                if (int.TryParse(text, out var year))
                {
                    var yearsSince = yearNow - year;
                    result = result.Replace($"[{text}]", yearsSince.ToString());
                }
            }

            return result;
        }

        public int HeightInPercentage {
            get
            {
                var dayOfWeek = (int)(DateInCalendar ?? DateHelper.FromKey(Date)).DayOfWeek;
                dayOfWeek = dayOfWeek == 0 ? 7 : dayOfWeek;
                var max = 8 - dayOfWeek;
                return Math.Min(DurationInDays, max) * 100;
            }
        }
    }
}
