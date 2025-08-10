namespace HomePage
{
    public class DayFood : SaveableItem
    {
        public string Key
        {
            get
            {
                return DateHelper.ToKey(Day);
            }
            set
            {
                Day = DateHelper.FromKey(value);
            }
        }

        public DateTime Day { get; set; }

        [SaveProperty]
        public string FoodId { get; set; }

        public Food Food { get; set; }
    }
}
