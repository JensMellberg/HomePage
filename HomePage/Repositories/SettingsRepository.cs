using HomePage.Data;
using HomePage.Model;
using Microsoft.EntityFrameworkCore;

namespace HomePage.Repositories
{
    public class SettingsRepository(IServiceScopeFactory scopeFactory, AppDbContext dbContext, IConfiguration config, DatabaseLogger logger)
    {
        public Settings Settings => dbContext.Settings.Single();

        public void PerformBackupAsync(bool force)
        {
            Task.Run(() => PerformBackup(force));
        }

        public Stream? GetLatestBackup(out string fileName)
        {
            var backupFolder = config["BackupFolderPath"];
            if (backupFolder == null)
            {
                logger.Warning("Backup folder is not configured.", null);
                fileName = null;
                return null;
            }

            var latestBackup = Directory.GetFiles(backupFolder, "*.bak")
                               .Select(f => new FileInfo(f))
                               .OrderByDescending(fi => fi.LastWriteTime)
                               .FirstOrDefault();

            if (latestBackup == null)
            {
                logger.Error("Could not find any backup files.", null);
                fileName = null;
                return null;
            }

            fileName = latestBackup.Name;
            return new FileStream(latestBackup.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public void PerformBackup(bool force)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var settings = db.Settings.Single();
            if (force || settings.ShouldBackUp(DateHelper.DateNow))
            {
                var backupFolder = config["BackupSqlFolderPath"];
                if (backupFolder == null)
                {
                    logger.Warning("Backup folder is not configured. Cannot perform backup.", null);
                    return;
                }

                var timeStamp = DateHelper.DateTimeNow.ToString("yy-MM-dd-HH-mm-ss");
                var filePath = Path.Combine(backupFolder, $"Backup{timeStamp}.bak");

                var sql = @"
                    BACKUP DATABASE [" + db.Database.GetDbConnection().Database + @"]
                    TO DISK = {0}
                    WITH INIT, COMPRESSION;
                ";

                db.Database.ExecuteSqlRaw(sql, filePath);
                settings.LastBackupDate = DateHelper.DateTimeNow;
                db.SaveChanges();

                DeleteOldBackups();
            }
        }

        public void DeleteOldBackups(int days = 5)
        {
            var backupFolder = config["BackupFolderPath"];
            if (backupFolder == null)
            {
                logger.Warning("Backup folder is not configured. Cannot perform backup.", null);
                return;
            }

            var cutoff = DateHelper.DateTimeNow.AddDays(-days);

            foreach (var file in Directory.GetFiles(backupFolder, "*.bak"))
            {
                var fi = new FileInfo(file);

                if (fi.LastWriteTime < cutoff)
                {
                    try
                    {
                        fi.Delete();
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogRowSeverity.Error, ex.Message, null, ex.StackTrace);
                    }
                }
            }
        }
    }
}
