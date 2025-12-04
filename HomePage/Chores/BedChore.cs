using HomePage.Model;

namespace HomePage.Chores
{
    public class BedSheetChore(ChoreModel source) : SinglePersonChore(source)
    {
        public const string ChoreId = "Bed";

        protected override int DaysBetween => 14;

        protected override string PersonName => Person.Anna.Name;
    }
}
