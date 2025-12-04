using HomePage.Model;

namespace HomePage.Chores
{
    public class SinkChore(ChoreModel source) : SinglePersonChore(source)
    {
        public const string ChoreId = "Sink";

        protected override string PersonName => Person.Jens.Name;

        protected override int DaysBetween => 93;
    }
}
