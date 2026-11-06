using HomePage.Data;
using HomePage.Model;

namespace HomePage.Achievements
{
    public class AnimalFriend : Achievement
    {
        public override string Title => "Djurvän";

        public override string Description => "Ät en vegetarisk rätt 10 veckor i rad";

        public override int Points => 20;

        public override string Icon => "🐇";

        public override int Id => 6;

        public override bool AdminOnly => true;

        public static bool WasEarned(DateTime date, AppDbContext dbContext, List<DayFood> allDayFood)
        {
            var firstOfWeek = DateHelper.GetFirstOfWeek(date);
            var totalVego = 0;
            for (var weekDelta = -1; weekDelta > -10; weekDelta--)
            {
                var weekDate = firstOfWeek.AddDays(7 * weekDelta);
                var hasVego = false;
                for (var i = 0; i < 7; i++)
                {
                    var crnt = weekDate.AddDays(i);
                    var dayFoodOfDay = allDayFood
                        .FirstOrDefault(x => x.Date == crnt);
                    if (dayFoodOfDay == null)
                    {
                        continue;
                    }

                    if (dayFoodOfDay.GetCategories(dbContext).Any(x => x.Name == "Vegetariskt"))
                    {
                        hasVego = true;
                        break;
                    }
                }

                if (!hasVego)
                {
                    break;
                }
                else
                {
                    totalVego++;
                }
            }

            return totalVego >= 10;
        }
    }
}
