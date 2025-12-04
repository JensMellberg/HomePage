namespace HomePage
{
    public class BruteForceProtector
    {
        private List<(string ip, DateTime date)> failedLogins = [];

        public bool ShouldBlockLogin(string ip)
        {
            lock (failedLogins)
            {
                failedLogins = failedLogins.Where(x => x.date > DateHelper.DateTimeNow.AddMinutes(-5)).ToList();
                if (failedLogins.Count(x => x.ip == ip) > 4)
                {
                    return true;
                }
            }

            return false;
        }

        public void AddFailedLogin(string ip)
        {
            lock (failedLogins)
            {
                failedLogins.Add((ip, DateHelper.DateTimeNow));
            }
        }
    }
}
