using System.Text.Json;
using HomePage.LogoGame;

namespace HomePage.Repositories
{
    public class LogotypeRepository
    {
        private const string RootFolder = "Logotypes";

        public IEnumerable<LogotypeMetadata> LoadLogos()
        {
            CreateFolderIfMissing();
            foreach (var logoFile in Directory.EnumerateFiles(RootFolder, "*.json"))
            {
                yield return LoadLogo(logoFile);
            }
        }

        public LogotypeMetadata LoadLogoFromId(string id) => LoadLogo(Path.Combine(RootFolder, $"{id}.json"));

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

        private static void CreateFolderIfMissing()
        {
            if (!Directory.Exists(RootFolder))
            {
                Directory.CreateDirectory(RootFolder);
            }
        }
    }
}
