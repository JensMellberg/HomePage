using System.Text;

namespace HomePage
{
    public class ExtraWord : SaveableItem
    {
        public string Key { get; set; }

        public string Word => Key;

        [SaveProperty]
        public string Creator { get; set; }

        [SaveProperty]
        public bool JensApproved { get; set; }

        [SaveProperty]
        public bool AnnaApproved { get; set; }
    }
}
