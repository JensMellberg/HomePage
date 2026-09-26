using Microsoft.EntityFrameworkCore;

namespace HomePage.Model
{
    [PrimaryKey(nameof(AchievementId), nameof(UserId))]
    public class UserAchievement
    {
        public int AchievementId { get; set; }

        public required string UserId { get; set; }

        public UserInfo User { get; set; }

        public DateTime DateEarned { get; set; }
    }
}
