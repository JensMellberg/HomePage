using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HomePage.Model
{
    public class CalendarActivity
    {
        [Key]
        [MaxLength(50)]
        public string Key { get; set; } = Guid.NewGuid().ToString();

        [MaxLength(50)]
        public string Text { get; set; }

        [MaxLength(50)]
        public string Person { get; set; }

        public DateTime CalendarDate { get; set; }

        public int DurationInDays { get; set; } = 1;

        public bool IsReoccuring { get; set; }

        public bool IsVacation { get; set; }

        [NotMapped]
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

        public int HeightInPercentage
        {
            get
            {
                var dayOfWeek = (int)(DateInCalendar ?? CalendarDate).DayOfWeek;
                dayOfWeek = dayOfWeek == 0 ? 7 : dayOfWeek;
                var max = 8 - dayOfWeek;
                return Math.Min(DurationInDays, max) * 100;
            }
        }
    }
}
