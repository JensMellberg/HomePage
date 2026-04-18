using HomePage.Model;

namespace HomePage.Chores
{
    public class BedSheetChore(ChoreModel source) : BaseChore(source)
    {
        public const string ChoreId = "Bed";

        protected override DateTime GetNextDate() => GetLastUpdatedDate.AddDays(14);
    }
}
