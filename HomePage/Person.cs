using HomePage.Model;

namespace HomePage
{
    public class Person
    {
        public required string Name { get; set; }

        public required string UserName { get; set; }

        public required string PhotoUrl { get; set; }

        public bool IsAdmin { get; set; }

        public static Person Jens = new() { Name = "Jens", UserName = "Jens", PhotoUrl = "8d0af03a-3f97-4fa8-b660-65b9fe977525.png", IsAdmin = true };

        public static Person Anna = new() { Name = "Anna", UserName = "Anna", PhotoUrl = "41871bc8-2e09-4e61-8c90-848ded114d2c.png", IsAdmin = true };

        public static Person MrRobot = new() { Name = "Mr. Robot", UserName = "MrRobot", PhotoUrl = "36c472ea-cee5-4c28-aa65-ba04cb354b2a.jpg" };

        public string GetPhotoHtml => $"<div class=\"profile-photo\" style=\"background-image: url(UserPhotos/{PhotoUrl});\"></div>";

        public string GetPhotoHtmlWithLink => $"<a " +
            $"class=\"profile-photo\" " +
            $"style=\"background-image: url(UserPhotos/{PhotoUrl});\"" +
            $"{GetProfileHref}></a>";

        public string GetProfileHref => $"href=\"Profile?userName={UserName}\"";

        public static string HtmlColorFromPerson(string person) => person switch
        {
            "Jens" => "#04b528a8",
            "Anna" => "#ab0ffff2",
            _ => "#02af95d9",
        };

        public static string HtmlTextColorFromPerson(string person) => person switch
        {
            "Jens" => "#099121",
            "Anna" => "#ab0ffff2",
            _ => "#007083",
        };

        public static Person FromUserInfo(UserInfo user) => user.UserName switch
        {
            "Jens" => Jens,
            "Anna" => Anna,
            _ => new Person { Name = user.DisplayName, UserName = user.UserName, PhotoUrl = user.PhotoUrl ?? MrRobot.PhotoUrl },
        };

        public override string ToString() => Name;
    }
}
