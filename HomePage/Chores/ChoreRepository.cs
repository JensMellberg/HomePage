using HomePage.Data;
using Microsoft.EntityFrameworkCore;

namespace HomePage.Chores
{
    public class ChoreRepository(AppDbContext dbContext)
    {
        private static readonly string[] ChoreIds = [FlowerChore.ChoreId, BedSheetChore.ChoreId, EyeChore.ChoreId,
            FlossChore.ChoreId, FlossChoreJens.ChoreId, SinkChore.ChoreId, WorkoutChore.ChoreId];

        public IEnumerable<BaseChore> GetAllChores() => ChoreIds.Select(GetChore)!;

        public BaseChore? GetChore(string choreId)
        {
            if (!ChoreIds.Contains(choreId))
            {
                return null;
            }

            var model = dbContext.ChoreModel.Include(x => x.Streaks).FirstOrDefault(x => x.Id == choreId);
            if (model == null)
            {
                model = new Model.ChoreModel
                {
                    Id = choreId,
                    LastUpdated = DateTime.MinValue,
                    Streaks =
                    [
                        new() { ChoreId = choreId, Streak = 0, Person = Person.Anna.Name },
                        new() { ChoreId = choreId, Streak = 0, Person = Person.Jens.Name }
                    ]
                };

                dbContext.ChoreModel.Add(model);
                dbContext.SaveChanges();
            }

            return choreId switch
            {
                FlowerChore.ChoreId => new FlowerChore(model),
                EyeChore.ChoreId => new EyeChore(model),
                FlossChore.ChoreId => new FlossChore(model),
                FlossChoreJens.ChoreId => new FlossChoreJens(model),
                WorkoutChore.ChoreId => new WorkoutChore(model),
                SinkChore.ChoreId => new SinkChore(model),
                BedSheetChore.ChoreId => new BedSheetChore(model),
                _ => throw new Exception($"Uknown chore id {choreId}"),
            };
        }
    }
}
