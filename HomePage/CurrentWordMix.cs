namespace HomePage
{
    public class CurrentWordMix : SaveableItem
    {
        public string Key { get; set; } = "Single";

        [SaveProperty]
        public string CreatedDate { get; set; }

        [SaveProperty]
        public string Letters { get; set; }

        public bool ShouldRecreate => DateHelper.FromKey(CreatedDate).Date < DateHelper.DateNow.Date;
    }
}
