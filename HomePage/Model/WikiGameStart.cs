using System.ComponentModel.DataAnnotations;

namespace HomePage.Model
{
    public enum WikiGameGoalDifficulty
    {
        Easy,
        Normal,
        Hard,
        Extreme,
        Unknown = -1
    }

    public static class WikiGameGoalDifficultyExtensions
    {
        public static string ToReadable(this WikiGameGoalDifficulty value) => value switch
        {
            WikiGameGoalDifficulty.Easy => "Lätt",
            WikiGameGoalDifficulty.Normal => "Medel",
            WikiGameGoalDifficulty.Hard => "Svår",
            WikiGameGoalDifficulty.Extreme => "Skitsvår",
            WikiGameGoalDifficulty.Unknown => "Okänd",
            _ => throw new NotImplementedException(),
        };
    }

    public class WikiGameStart
    {
        [Key]
        public DateTime Date { get; set; } = DateHelper.DateNow;

        [MaxLength(2000)]
        public required string Title { get; set; }

        [MaxLength(2000)]
        public required string GoalTitle { get; set; }

        public required string Summary { get; set; }

        [MaxLength(10)]
        public required string Language { get; set; }

        public WikiGameGoalDifficulty GoalDifficulty { get; set; }

        public string GoalTitleWithDifficulty => GoalTitle.Replace("_", " ") + " (" + GoalDifficulty.ToReadable() + ")";
    }
}
