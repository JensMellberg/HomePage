namespace HomePage
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SavePropertyAttribute : Attribute
    {
        public SavePropertyAttribute() { }
    }
}
