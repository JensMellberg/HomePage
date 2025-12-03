using HomePage.Data;

namespace HomePage.Spending
{
    public class SpendingGroupRepository(AppDbContext dbContext)
    {
        public IEnumerable<ISpendingGroup> GetAllSpendingGroups(string person, DateTime start, DateTime end)
        {
            var allGroups = dbContext.SpendingGroup.Where(x => x.Person == person).ToList();

            return allGroups
                .Where(x => x.IsDateBasedGroup)
                .Select(x => new DateBasedSpendingGroup(x))
                .Where(x => x.startDate <= end && x.endDate >= start)
                .Concat(allGroups
                .Where(x => !x.IsDateBasedGroup)
                .Select(x => new UserBasedSpendingGroup(x))
                .OfType<ISpendingGroup>()
                .Concat([new OtherSpendingGroup()])
                .OrderBy(x => x.SortOrder));
        }  
    }
}
