using System.IO.Compression;

namespace HomePage
{
    public class SettingsRepositoryTemp : SingleRepository<SettingsTemp>
    {
        public override string FileName => "Settings.txt";

        public static FlossChore FlossChore => new();

        public static FlossChoreJens FlossChoreJens => new();

        public static FlowerChore FlowerChore => new();

        public static EyeChore EyeChore => new();

        public static SinkChore SinkChore => new();

        public static WorkoutChore WorkoutChore => new();

        public static BedSheetChore BedSheetChore => new();

        public void PerformBackupAsync(bool force)
        {
            Task.Run(() => PerformBackup(force));
        }

        public string PerformBackup(bool force)
        {
            var settings = this.Get();
            if (force || settings.ShouldBackUp(DateTime.Now))
            {
                if (!Directory.Exists("Backups"))
                {
                    Directory.CreateDirectory("Backups");
                }

                var timeStamp = DateTime.Now.ToString("yy-MM-dd-HH-mm-ss");
                var fileName = @"Backups/Backup" + timeStamp + ".zip";
                ZipFile.CreateFromDirectory("Database", fileName);
                settings.LastBackup = DateHelper.ToKey(DateTime.Now);
                this.Save(settings);
                return fileName;
            }

            return null;
        }
    }
}
