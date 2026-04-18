using HomePage.Data;
using HomePage.Model;
using HomePage.WikiGame;

namespace HomePage.Repositories
{
    public class WikiGameRepository(AppDbContext dbContext, DatabaseLogger logger)
    {
        public const string GoalPageTitle = "Adolf_Hitler";
        public (string html, int steps, List<string> allowedLinks) GetCurrentUserPage(string username)
        {
            var playerNavigations = GetNavigations(username, DateHelper.DateNow);
            var steps = 0;
            CachedWikiGamePage page;
            if (playerNavigations.Any())
            {
                var lastNavigation = playerNavigations.Last();
                page = GetPageContent(lastNavigation.Title);
                steps = lastNavigation.Step;
            } 
            else
            {
                page = GetPageContent(GetStartPageTitle(DateHelper.DateNow));
            }
            
            return (page.PageContent, steps, page.AllowedLinks);
        }

        public (string html, int steps, bool isWin)? NavigateToPage(string title, string username)
        {
            var currentPage = GetCurrentUserPage(username);
            if (!currentPage.allowedLinks.Contains(title))
            {
                logger.Error($"{username} cannot navigate to page {title} since there is no outgoing links to it.", username);
                return null;
            }

            var newPage = GetPageContent(title);
            dbContext.WikiGameNavigations.Add(new WikiGameNavigation { Title = title, UserName = username, Step = currentPage.steps + 1 });
            dbContext.SaveChanges();
            return (newPage.PageContent, currentPage.steps + 1, title == GoalPageTitle);
        }

        public List<WikiGameNavigation> GetNavigations(string username, DateTime date)
        {
            return dbContext.WikiGameNavigations
                .Where(x => x.UserName == username && x.Date == date)
                .OrderBy(x => x.Step)
                .ToList();
        }

        public string GetStartPageTitle(DateTime date)
        {
            var title = dbContext.WikiGameStarts.FirstOrDefault(x => x.Date == date);
            if (title == null) {
                var newTitle = WikipediaClient.GetRandomTitle();
                var newStart = new WikiGameStart { Title = newTitle };
                dbContext.WikiGameStarts.Add(newStart);
                CleanUpCache();
                dbContext.SaveChanges();
                title = newStart;
            }

            return title.Title;
        }

        public CachedWikiGamePage GetPageContent(string title)
        {
            var page = dbContext.CachedWikiGamePages.Find(title);
            if (page != null)
            {
                page.LastUsed = DateHelper.DateTimeNow;
                dbContext.SaveChanges();
                return page;
            }

            var content = WikipediaClient.GetPageContent(title);
            var convertedContent = WikiGameHtmlHelper.ConvertHtml(content, title == GoalPageTitle);
            var cachedPage = new CachedWikiGamePage { 
                PageContent = convertedContent.convertedHtml,
                Title = title,
                AllowedLinks = title == GoalPageTitle ? [] : convertedContent.allowedLinks
            };

            dbContext.CachedWikiGamePages.Add(cachedPage);
            dbContext.SaveChanges();
            return cachedPage;
        }

        public List<WikiGameDayResult> GetAllResults()
        {
            var results = new List<WikiGameDayResult>();
            var daysWithGames = dbContext.WikiGameStarts.OrderByDescending(x => x.Date).ToList();
            foreach (var dayStart in daysWithGames)
            {
                var usersThatFinished = dbContext.WikiGameNavigations
                    .Where(x => x.Date == dayStart.Date && x.Title == GoalPageTitle)
                    .Select(x => x.UserName)
                    .ToList();
                if (!usersThatFinished.Any() && DateHelper.DateNow != dayStart.Date)
                {
                    continue;
                }

                results.Add(new WikiGameDayResult
                {
                    Date = dayStart.Date,
                    Results = usersThatFinished
                        .Select(x => GetUserResultForDate(x, dayStart.Date, dayStart.Title)!)
                        .OrderBy(x => x.Steps)
                        .ToList()
                });
            }

            return results;
        }

        public bool UserHasResultToday(string? username) => GetUserResultForDate(username, DateHelper.DateNow) != null;

        public WikiGameUserResult? GetUserResultForDate(string? username, DateTime date)
        {
            var gameStart = dbContext.WikiGameStarts.FirstOrDefault(x => x.Date == date);
            return GetUserResultForDate(username, date, gameStart?.Title);
        }

        public WikiGameUserResult? GetUserResultForDate(string? username, DateTime date, string? startTitle)
        {
            if (startTitle == null || username == null)
            {
                return null;
            }

            var navigations = GetNavigations(username, date);
            if (!navigations.Any())
            {
                return null;
            }

            var path = navigations.Select(x => x.Title).ToList();
            path.Insert(0, startTitle);
            var steps = navigations.Last().Step;
            if (navigations.Last().Title != GoalPageTitle)
            {
                return null;
            }

            return new WikiGameUserResult
            {
                PathTaken = path,
                Steps = steps,
                UserName = username
            };
        }

        private void CleanUpCache()
        {
            var oldEntries = dbContext.CachedWikiGamePages.Where(x => x.LastUsed.AddDays(7) < DateHelper.DateTimeNow).ToList();
            dbContext.CachedWikiGamePages.RemoveRange(oldEntries);
        }
    }
}
