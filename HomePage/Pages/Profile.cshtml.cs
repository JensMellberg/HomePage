using HomePage.Achievements;
using HomePage.Chores;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    public class ProfileModel(ChoreRepository choreRepository, AchievementRepository achievementRepository, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public List<(string choreId, int total, string longest)> ChoreStats = [];

        public required Person Person { get; set; }

        public required List<UserAchievement> UserAchievements { get; set; }

        public IActionResult OnGet(string userName)
        {
            var userInfo = SignInRepository.GetUserInfo(userName);
            if (userInfo == null)
            {
                return NotFound();
            }

            Person = Person.FromUserInfo(userInfo);
            if (userInfo.IsAdmin)
            {
                var allChores = choreRepository.GetAllChores();
                foreach (var chore in allChores.Where(x => x.PersonUsesChore(userInfo.UserName))) {
                    var streak = chore.GetStreak(userInfo.UserName);
                    var highestStreak = chore.HasStreak ? streak.HighestStreak.ToString() : "-";
                    ChoreStats.Add((chore.Id, streak.TotalPerformed, highestStreak));
                }
            }

            UserAchievements = achievementRepository.GetUserAchievements(Person).ToList();

            return Page();
        }
    }
}
