namespace HomePage
{
    public class UserContext(IHttpContextAccessor httpContextAccessor)
    {
        public Guid? UserGroupId => GuidFromSession("GroupId");

        public Guid? ImpersonatingUserGroupId => GuidFromSession("ImpersonateGroupId");

        private Guid? GuidFromSession(string key)
        {
            var httpContext = httpContextAccessor.HttpContext;
            var value = httpContext?.Session?.GetString(key);

            return value == null ? null : Guid.Parse(value);
        }
    }
}
