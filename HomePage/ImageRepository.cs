using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Png;
using MetadataExtractor;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace HomePage
{
    public class ImageRepository
    {
        public static ImageRepository Instance => instance ??= new ImageRepository();

        private static ImageRepository instance;

        private DateTime lastUpdated = DateTime.MinValue;
        private const int UpdateIntervalSeconds = 30;
        private const string DirectoryPath = "Pictures";
        private string imageUrl;
        private string imageTaken;

        public (IEnumerable<string>, long totalSize) GetAll()
        {
            if (!System.IO.Directory.Exists(DirectoryPath))
            {
                System.IO.Directory.CreateDirectory(DirectoryPath);
            }

            var allPictures = System.IO.Directory.EnumerateFiles(DirectoryPath);

            return (allPictures, new DirectoryInfo(DirectoryPath).GetFiles().Sum(x => x.Length));
        }

        public void DeleteImage(string path)
        {
            if (!System.IO.Directory.Exists(DirectoryPath))
            {
                System.IO.Directory.CreateDirectory(DirectoryPath);
            }

            var combinedPath = Path.Combine(DirectoryPath, path);
            if (File.Exists(combinedPath))
            {
                File.Delete(combinedPath);
            }
        }

        public int SecondsUntilNextUpdate => (lastUpdated.AddSeconds(UpdateIntervalSeconds) - DateTime.Now).Seconds + 1;

        public async Task UploadFiles(IEnumerable<IFormFile> files)
        {
            if (!System.IO.Directory.Exists(DirectoryPath))
            {
                System.IO.Directory.CreateDirectory(DirectoryPath);
            }

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.FileName);

                var randomFileName = $"{Guid.NewGuid()}{extension}";

                var filePath = Path.Combine(DirectoryPath, randomFileName);

                using var stream = file.OpenReadStream();

                using (var image = Image.Load(stream))
                {
                    int maxWidth = 800;
                    if (file.Length > 700 && image.Width > maxWidth)
                    {
                        image.Mutate(x => x.Resize(maxWidth, 0));
                    }

                    int maxHeight = 1400;
                    if (file.Length > 700 && image.Height > maxHeight)
                    {
                        image.Mutate(x => x.Resize(0, maxHeight));
                    }

                    await image.SaveAsync(filePath);
                }
            }
        }

        public (string, string) GetImageUrl()
        {
            Update();
            return (imageUrl, imageTaken);
        }

        private void Update()
        {
            if (DateTime.Now > lastUpdated.AddSeconds(UpdateIntervalSeconds))
            {
                lastUpdated = DateTime.Now;
                if (!System.IO.Directory.Exists(DirectoryPath))
                {
                    System.IO.Directory.CreateDirectory(DirectoryPath);
                }

                var allPictures = System.IO.Directory.EnumerateFiles(DirectoryPath).ToList();
                if (allPictures.Count == 0) return;

                var index = new Random().Next(allPictures.Count);
                imageUrl = allPictures[index];
                var directories = ImageMetadataReader.ReadMetadata(imageUrl);
                var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

                try
                {
                    var dateTaken = subIfdDirectory?.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal);

                    if (dateTaken != null)
                    {
                        imageTaken = $"{DateHelper.MonthNumberToString[dateTaken.Value.Month]} {dateTaken.Value.Year}";
                    }
                } catch { }
            }
        }
    }
}
