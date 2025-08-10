namespace HomePage.Spending
{
    public class SpendingGroupRepository : Repository<SpendingGroup>
    {
        public override string FileName => "SpendingGroup.txt";

        public IEnumerable<ISpendingGroup> GetAllSpendingGroups(string person, DateTime start, DateTime end)
        {
            var allGroups = this.GetValues().Values.Where(x => x.Person == person).ToList();

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
