namespace HomePage
{
    public class FoodRanking : SaveableItem
    {
        public string Key
        {
            get
            {
                return MakeId(Day, Person);
            }
            set
            {
                var tokens = value.Split('|');
                Day = tokens[0];
                Person = tokens[1];
            }
        }

        public static string MakeId(DateTime date, string person) => MakeId(DateHelper.ToKey(date), person);

        public static string MakeId(string date, string person) => date + "|" + person;

        public string RankingText => string.IsNullOrEmpty(FoodId) ? "-" : Ranking.ToString();

        [SaveProperty]
        public string Day { get; set; }

        [SaveProperty]
        public string Person { get; set; }

        [SaveProperty]
        public string FoodId { get; set; }

        [SaveProperty]
        public int Ranking { get; set; }
    }
}
