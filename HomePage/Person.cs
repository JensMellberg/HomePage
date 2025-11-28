namespace HomePage
{
    public class Person
    {
        public required string Name { get; set; }

        public static Person Jens = new() { Name = "Jens" };

        public static Person Anna = new() { Name = "Anna" };

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

        public static Person FromString(string s) => s switch
        {
            "Jens" => Jens,
            "Anna" => Anna,
            _ => throw new Exception(),
        };

        public override string ToString() => Name;
    }
}
