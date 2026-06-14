using HomePage.Data;
using HomePage.Model;
using HomePage.WikiGame;

namespace HomePage.Repositories
{
    public class WikiGameRepository(AppDbContext dbContext, DatabaseLogger logger)
    {
        public (string html, int steps, List<string> allowedLinks, Guid? lastNavigationId, string title) GetCurrentUserPage(string username)
        {
            var startPage = GetStartPage(DateHelper.DateNow);
            var playerNavigations = GetNavigations(username, DateHelper.DateNow);
            var steps = 0;
            CachedWikiGamePage page;
            Guid? lastNavigationId = null;
            if (playerNavigations.Any())
            {
                var lastNavigation = playerNavigations.Last();
                page = GetPageContent(lastNavigation.Title, startPage.Language);
                steps = lastNavigation.Step;
                lastNavigationId = lastNavigation.Id;
            } 
            else
            {
                page = GetPageContent(GetStartPage(DateHelper.DateNow).Title, startPage.Language);
            }
            
            return (page.PageContent, steps, page.AllowedLinks, lastNavigationId, page.Title);
        }

        public WikiGameNavigationResult? NavigateToPage(string title, string username)
        {
            var startPage = GetStartPage(DateHelper.DateNow);
            var goal = startPage.GoalTitle;
            var currentPage = GetCurrentUserPage(username);
            if (!currentPage.allowedLinks.Contains(title))
            {
                logger.Error($"{username} cannot navigate to page {title} since there is no outgoing links to it.", username);
                return null;
            }

            (var readableGoalTitle, var _) = WikipediaClient.GetPageInfo(goal, startPage.Language);
            var nextPageInfo = WikipediaClient.GetPageInfo(title, startPage.Language);
            var isWin = nextPageInfo.title == readableGoalTitle;

            var newPage = GetPageContent(title, startPage.Language, isWin);
            
            Guid? backId = null;
            if (!isWin)
            {
                backId = currentPage.steps == 0 ? Guid.Empty : currentPage.lastNavigationId;
            }

            dbContext.WikiGameNavigations.Add(new WikiGameNavigation 
            { 
                Title = title, 
                UserName = username, 
                Step = currentPage.steps + 1,
                BackId = backId
            });

            dbContext.SaveChanges();
            return new WikiGameNavigationResult
            {
                Html = newPage.PageContent,
                Steps = currentPage.steps + 1,
                IsWin = isWin,
                CanGoBack = !isWin,
                Title = title
            };
        }

        public WikiGameNavigationResult? NavigateBackwards(string username)
        {
            var startPage = GetStartPage(DateHelper.DateNow);
            var navigations = GetNavigations(username, DateHelper.DateNow);
            if (!navigations.Any() || navigations.Last().Title == startPage.GoalTitle)
            {
                return null;
            }

            var lastNavigation = navigations.Last();
            var backId = lastNavigation.BackId;
            if (backId == null)
            {
                return null;
            }

            var backNavigation = navigations.FirstOrDefault(x => x.Id == backId) ?? new WikiGameNavigation { Title = startPage.Title, UserName = "", BackId = Guid.Empty};

            var newPage = GetPageContent(backNavigation.Title, startPage.Language);
            dbContext.WikiGameNavigations.Add(new WikiGameNavigation 
            { 
                Title = newPage.Title, 
                UserName = username,
                Step = lastNavigation.Step + 1,
                BackId = backNavigation.BackId
            });
            dbContext.SaveChanges();
            return new WikiGameNavigationResult
            {
                Html = newPage.PageContent,
                Steps = lastNavigation.Step + 1,
                IsWin = false,
                CanGoBack = backId != null && backId != Guid.Empty,
                Title = newPage.Title
            };
        }

        public List<WikiGameNavigation> GetNavigations(string username, DateTime date)
        {
            return dbContext.WikiGameNavigations
                .Where(x => x.UserName == username && x.Date == date)
                .OrderBy(x => x.Step)
                .ToList();
        }

        public WikiGameStart GetStartPage(DateTime date)
        {
            var start = dbContext.WikiGameStarts.FirstOrDefault(x => x.Date == date);
            if (start == null) {
                var goalTitle = GetNewGoalTitle();
                var newTitle = WikipediaClient.GetRandomTitle(goalTitle.language);
                
                var newStart = new WikiGameStart { Title = newTitle, GoalTitle = goalTitle.title, Language = goalTitle.language, Summary = goalTitle.summary };
                dbContext.WikiGameStarts.Add(newStart);
                CleanUpCache();
                dbContext.SaveChanges();
                start = newStart;
            }

            return start;
        }

        public CachedWikiGamePage GetPageContent(string title, string language, bool isGoal = false)
        {
            var page = dbContext.CachedWikiGamePages.Find(title);
            var startPage = GetStartPage(DateHelper.DateNow);
            if (page != null)
            {
                if (startPage.GoalTitle == page.Title && page.LastUsed < DateHelper.DateNow)
                {
                    dbContext.CachedWikiGamePages.Remove(page);
                    dbContext.SaveChanges();
                } else
                {
                    page.LastUsed = DateHelper.DateTimeNow;
                    dbContext.SaveChanges();
                    return page;
                }
            }

            var content = WikipediaClient.GetPageContent(title, language);
            if (content == null)
            {
                throw new Exception($"No content from wikipedia page {title}");
            }

            var convertedContent = WikiGameHtmlHelper.ConvertHtml(content, title == startPage.GoalTitle || isGoal);
            var cachedPage = new CachedWikiGamePage { 
                PageContent = convertedContent.convertedHtml,
                Title = title,
                AllowedLinks = title == startPage.GoalTitle ? [] : convertedContent.allowedLinks
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
                var usersWithResults = dbContext.WikiGameNavigations
                    .Where(x => x.Date == dayStart.Date)
                    .Select(x => x.UserName)
                    .Distinct()
                    .ToList();
                var isToday = DateHelper.DateNow == dayStart.Date;
                if (!usersWithResults.Any() && !isToday)
                {
                    continue;
                }


                results.Add(new WikiGameDayResult
                {
                    Date = dayStart.Date,
                    GoalTitle = dayStart.GoalTitle.Replace('_', ' '),
                    Results = usersWithResults
                        .Select(x => GetUserResultForDate(x, dayStart.Date, dayStart.Title)!)
                        .Where(x => x != null && (!isToday || x.HasFinished))
                        .OrderBy(x => x.HasFinished ? 0 : 1)
                        .ThenBy(x => x.Steps)
                        .ToList()
                });
            }

            return results;
        }

        public bool UserHasFinishedToday(string? username) => GetUserResultForDate(username, DateHelper.DateNow)?.HasFinished == true;

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
            var isWin = navigations.Last().IsWinNavigation;

            return new WikiGameUserResult
            {
                PathTaken = path,
                Steps = steps,
                UserName = username,
                HasFinished = isWin
            };
        }

        public List<WikiGameSuggestion> GetSuggestions()
        {
            return dbContext.WikiGameSuggestions.OrderByDescending(x => x.Votes).ThenBy(x => x.Title).ToList();
        }

        private void CleanUpCache()
        {
            var oldEntries = dbContext.CachedWikiGamePages.Where(x => x.LastUsed.AddDays(7) < DateHelper.DateTimeNow).ToList();
            dbContext.CachedWikiGamePages.RemoveRange(oldEntries);
        }

        private (string title, string language, string summary) GetNewGoalTitle()
        {
            var allSuggestions = GetSuggestions();
            if (allSuggestions.Count == 0)
            {
                return ("Adolf_Hitler", "en", "");
            }

            var mostVotes = allSuggestions.First().Votes;
            var suggestions = allSuggestions.Where(x => x.Votes == mostVotes).ToList();
            var index = Random.Shared.Next(suggestions.Count);
            var suggestion = suggestions[index];
            suggestion.Voters = [];
            suggestion.Votes = 0;

            (var readableTitle, var summary) = WikipediaClient.GetPageInfo(suggestion.Title, suggestion.Language);

            return (readableTitle, suggestions[index].Language, summary);
        }

    }
}
