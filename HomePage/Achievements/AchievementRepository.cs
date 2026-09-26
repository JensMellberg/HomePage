using HomePage.Data;

namespace HomePage.Achievements
{
    public class AchievementRepository(AppDbContext dbContext, DatabaseLogger logger)
    {
        public static Nattsurfare Nattsurfare = new();
        public static FinaTender FinaTender = new();
        public static LevandeOrdlista LevandeOrdlista = new();
        public static WikiNerd WikiNerd = new();
        public static GreenFingers GreenFingers = new();
        public static AnimalFriend AnimalFriend = new();
        public static QuickClick QuickClick = new();
        public static Reklamskadad Reklamskadad = new();

        public static Achievement[] AllAchievements = [
            FinaTender,
            Nattsurfare,
            WikiNerd,
            GreenFingers,
            LevandeOrdlista,
            QuickClick,
            AnimalFriend,
            Reklamskadad
        ];

        public IEnumerable<UserAchievement> GetUserAchievements(Person person)
        {
            var earnedAchievements = dbContext.UserAchievements.Where(x => x.UserId == person.UserName).Select(x => x.AchievementId).ToHashSet();
            return AllAchievements
                .Where(x => person.IsAdmin || !x.AdminOnly)
                .Select(x => new UserAchievement(x, earnedAchievements.Contains(x.Id)))
                .OrderBy(x => x.IsEarned ? 0 : 1)
                .ThenBy(x => x.Achievement.Points);
        }

        public void EarnAchievement(Achievement achievement, string userName)
        {
            if (dbContext.UserAchievements.Any(x => x.AchievementId == achievement.Id && x.UserId == userName)) {
                return;
            }

            logger.Information($"{userName} earned achievement {achievement.Title}.", userName);
            dbContext.UserAchievements.Add(new Model.UserAchievement
            {
                UserId = userName,
                AchievementId = achievement.Id,
                DateEarned = DateHelper.DateTimeNow
            });
            dbContext.SaveChanges();
        }
    }
}
