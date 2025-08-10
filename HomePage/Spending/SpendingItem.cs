namespace HomePage.Spending
{
    public class SpendingItem : SaveableItem
    {
        public string Key
        {
            get
            {
                return Person + '|' + Date + '|' + Place + '|' + Amount;
            }
            set
            {

            }
        }

        public DateTime ConvertedDate { get; set; }

        [SaveProperty]
        public string Person { get; set; }

        [SaveProperty]
        public string Date { get; set; }

        [SaveProperty]
        public string Place { get; set; }

        [SaveProperty]
        public int Amount { get; set; }

        [SaveProperty]
        public string SetGroupId { get; set; }
    }
}
