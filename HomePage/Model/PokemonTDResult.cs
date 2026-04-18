using System.ComponentModel.DataAnnotations;
using PokemonTDEngine;

namespace HomePage.Model
{
    public class PokemonTDResult
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(50)]
        public string Person { get; set; }

        public Difficulty Difficulty { get; set; }

        public long DamageTestResult { get; set; }

        public bool IsWin { get; set; }

        public int LevelCompleted { get; set; }

        public GameResult ToGameResult => new(LevelCompleted, DamageTestResult, Difficulty, IsWin);

        public static PokemonTDResult FromGameResult(GameResult gameResult, string person) => new()
        {
            IsWin = gameResult.IsWin,
            Difficulty = gameResult.Difficulty,
            DamageTestResult = gameResult.DamageTestResult,
            LevelCompleted = gameResult.LevelCompleted,
            Person = person
        };

        public override string ToString() => IsWin ? $"Completed! Damage result: {DamageTestResult}" : $"Level completed: {LevelCompleted}";
    }
}
