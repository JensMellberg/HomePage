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
        public string LastWorkoutTime { get; set; }

        public bool ShouldBackUp(DateTime date) => DateHelper.FromKey(LastBackup).Date < date.Date;
    }
}
