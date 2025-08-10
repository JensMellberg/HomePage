namespace HomePage
{
    public class SignInInfo : SaveableItem
    {
        public string Key { get; set; } = "Single";

        [SaveProperty]
        public string GptKey { get; set; }

        [SaveProperty]
        public string Password { get; set; }

        [SaveProperty]
        public string JensPassword { get; set; }

        [SaveProperty]
        [SaveAsList]
        public List<string> LoggedInCookies { get; set; } = new List<string>();

        [SaveProperty]
        [SaveAsList]
        public List<string> JensLoggedInCookies { get; set; } = new List<string>();
    }
}
