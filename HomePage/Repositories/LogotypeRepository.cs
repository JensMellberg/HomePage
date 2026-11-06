using System.Text.Json;
using HomePage.LogoGame;

namespace HomePage.Repositories
{
    public class LogotypeRepository
    {
        private const string RootFolder = "Logotypes";

        public IEnumerable<LogotypeMetadata> LoadLogos(string? creator = null)
        {
            CreateFolderIfMissing();
            foreach (var logoFile in Directory.EnumerateFiles(RootFolder, "*.json"))
            {
                var logo = LoadLogo(logoFile);
                if (creator == null || logo.GetCreator == creator)
                {
                    yield return logo;
                }
            }
        }

        public LogotypeMetadata LoadLogoFromId(string id) => LoadLogo(IdToPath(id));

        public LogotypeMetadata LoadLogo(string path)
        {
            if (!File.Exists(path))
            {
                throw new Exception($"Invalid logotype data at {path}");
            }

            var logoModel = JsonSerializer.Deserialize<LogotypeMetadata>(File.ReadAllText(path))
                   ?? throw new Exception($"Failed to parse logo file {path}.");
            logoModel.Id = Path.GetFileNameWithoutExtension(path);

            return logoModel;
        }

        public void SaveLogo(string[] answers, string userName, int hints, IFormFile logo, IFormFile? originalLogo)
        {
            var id = Guid.NewGuid().ToString();
            var path = Path.Combine(RootFolder, id + ".json");
            var logoUrl = SaveLogo(logo);
            var originalLogoUrl = originalLogo != null ? SaveLogo(originalLogo) : null;

            var json = JsonSerializer.Serialize(new
            {
                Answers = answers,
                LogoUrl = logoUrl,
                OriginalLogoUrl = originalLogoUrl,
                Creator = userName,
                HintsAvailable = hints
            });

            File.WriteAllText(path, json);

            string SaveLogo(IFormFile logoToSave)
            {
                var uniqueId = Guid.NewGuid().ToString() + Path.GetExtension(logoToSave.FileName);
                var filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "Logotypes",
                    uniqueId);
                using var stream = File.Create(filePath);
                logoToSave.CopyTo(stream);
                return uniqueId;
            }
        }

        public void DeleteLogo(string id)
        {
            var logo = LoadLogoFromId(id);
            DeleteLogoImage(logo.LogoUrl);
            if (logo.OriginalLogoUrl != null)
            {
                DeleteLogoImage(logo.OriginalLogoUrl);
            }

            File.Delete(IdToPath(id));

            void DeleteLogoImage(string id)
            {
                var filePath = Path.Combine(
                   Directory.GetCurrentDirectory(),
                   "wwwroot",
                   "Logotypes",
                   id);
                File.Delete(filePath);
            }
        }

        private static string IdToPath(string id) => Path.Combine(RootFolder, $"{id}.json");

        private static void CreateFolderIfMissing()
        {
            if (!Directory.Exists(RootFolder))
            {
                Directory.CreateDirectory(RootFolder);
            }
        }
    }
}
