namespace HomePage.Model
{
    public class Settings
    {
        public int Id { get; set; } = 1;

        public DateTime LastBackupDate { get; set; }

        public DateTime LastRedDayUpdateDate { get; set; }

        public bool ShouldBackUp(DateTime date) => LastBackupDate.Date < date.Date;
    }
}
