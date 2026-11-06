using HomePage.Repositories;

namespace HomePage.Achievements
{
    public class Taskig : Achievement
    {
        public override string Title => "Taskig";

        public override string Description => "Lägg till en logga som ingen klarar utan ledtrådar (men som någon klarar)";

        public override int Points => 15;

        public override string Icon => "🧱";

        public override int Id => 9;

        public override bool AdminOnly => false;

        public static string? WasEarnedYesterday(LogotypeResultRepository resultRepository)
        {
            var yesterday = DateHelper.DateNow.AddDays(-1);
            var yesterdayLogo = resultRepository.GetDayLogoMetadata(yesterday);
            if (yesterdayLogo == null)
            {
                return null;
            }

            var yesterdayResults = resultRepository.GetResultsForDay(yesterday).Where(x => x.UserName != yesterdayLogo.GetCreator);
            if (yesterdayResults.All(x => x.HintsUsed > 0) && yesterdayResults.Any())
            {
                return yesterdayLogo.GetCreator;
            }

            return null;
        }
    }
}
