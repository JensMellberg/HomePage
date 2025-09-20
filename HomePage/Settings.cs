namespace HomePage
{
    public class Settings : SaveableItem
    {
        public string Key { get; set; } = "Single";

        [SaveProperty]
        public string LastBackup { get; set; } = DateHelper.ToKey(new DateTime(2020, 01, 01));

        [SaveProperty]
        public int LastRedDayYearUpdate { get; set; }

        [SaveProperty]
        public string LastFlowerTime { get; set; }

        [SaveProperty]
        [SaveAsList]
        public List<string> Streaks { get; set; } = new List<string>();

        [SaveProperty]
        public string LastFlowerPerson { get; set; }

        [SaveProperty]
        public string LastBedTime { get; set; }

        [SaveProperty]
        public string LastEyeTime { get; set; }

        [SaveProperty]
        public string LastSinkTime { get; set; }


        [SaveProperty]
        public string LastFlossTime { get; set; }

        [SaveProperty]
        public string LastFlossTimeJens { get; set; }

        [SaveProperty]
        public string LastWorkoutTime { get; set; }

        public bool ShouldBackUp(DateTime date) => DateHelper.FromKey(LastBackup).Date < date.Date;
    }
}
