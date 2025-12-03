using HomePage.Model;

namespace HomePage
{
    public class Person
    {
        public required string Name { get; set; }

        public required string UserName { get; set; }

        public static Person Jens = new() { Name = "Jens", UserName = "Jens" };

        public static Person Anna = new() { Name = "Anna", UserName = "Anna" };

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
            _ => new Person { Name = user.DisplayName, UserName = user.UserName },
        };

        public override string ToString() => Name;
    }
}
