using HomePage.Data;
using HomePage.Model;
using HomePage.WikiGame;
using Microsoft.Extensions.Options;

namespace HomePage.Repositories
{
    public class WikiGameRepository(AppDbContext dbContext, DatabaseLogger logger, IOptions<RobotConfig> robotConfig, IServiceScopeFactory scopeFactory)
    {
        private const int MinimumGoalPageLinks = 100;
        private const int MinimumGoalPageViews = 15000;
        public (string html, int steps, List<string> allowedLinks, Guid? lastNavigationId, string title) GetCurrentUserPage(string username, DateTime date)
        {
            var startPage = GetStartPage(date);
            var playerNavigations = GetNavigations(username, date);
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
                page = GetPageContent(GetStartPage(date).Title, startPage.Language);
            }
            
            return (page.PageContent, steps, page.AllowedLinks, lastNavigationId, page.Title);
        }

        public WikiGameNavigationResult? NavigateToPage(string title, string username, DateTime date)
        {
            var startPage = GetStartPage(date);
            var goal = startPage.GoalTitle;
            var currentPage = GetCurrentUserPage(username, date);
            if (!currentPage.allowedLinks.Contains(title))
            {
                logger.Error($"{username} cannot navigate to page {title} since there is no outgoing links to it.", username);
                return null;
            }

            var readableGoalTitle = WikipediaClient.GetPageInfo(goal, startPage.Language).Title;
            var nextPageInfo = WikipediaClient.GetPageInfo(title, startPage.Language);
            var isWin = nextPageInfo.Title == readableGoalTitle;

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
                BackId = backId,
                Date = date
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
                var newTitle = WikipediaClient.GetRandomPage(goalTitle.language).Title;
                var difficulty = GetTitleDifficulty(goalTitle.title, goalTitle.language);

                var newStart = new WikiGameStart { 
                    Title = newTitle, 
                    GoalTitle = goalTitle.title, 
                    Language = goalTitle.language, 
                    Summary = goalTitle.summary,
                    GoalDifficulty = difficulty
                };

                dbContext.WikiGameStarts.Add(newStart);
                CleanUpCache();
                dbContext.SaveChanges();
                start = newStart;

                if (robotConfig.Value.RunWikiRace)
                {
                    AddRobotResult(date);
                }
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
                    GoalTitle = dayStart.GoalTitleWithDifficulty,
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

        public void AddRobotResult(DateTime date)
        {
            var start = dbContext.WikiGameStarts.FirstOrDefault(x => x.Date == date);
            if (start == null || dbContext.WikiGameNavigations.Where(x => x.Date == date && x.UserName == Person.MrRobot.UserName).Any())
            {
                return;
            }

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

                using var scope = scopeFactory.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<DatabaseLogger>();
                logger.Information($"Calculating best wikipedia path for date {date.ToReadable()}", null);
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var secondRepository = scope.ServiceProvider.GetRequiredService<WikiGameRepository>();
                var linksCache = scope.ServiceProvider.GetRequiredService<WikipediaLinksCache>();

                linksCache.CleanStaleEntries();
                var calculator = new WikipediaCalculator(start.Language, secondRepository, linksCache);
                using var cts = new CancellationTokenSource(TimeSpan.FromHours(2));
                try
                {
                    var result = calculator.FindOptimalPath(start.Title, start.GoalTitle, cts.Token);
                    if (result == null || result.Count == 0)
                    {
                        return;
                    }

                    logger.Information($"Optimal wikipedia path finished calculations. Made {calculator.TotalRequestsMade} requests. {linksCache.TotalCacheHits} cache hits.", null);
                    foreach (var navigation in result.Skip(1))
                    {
                        secondRepository.NavigateToPage(navigation.NormalizedTitle, Person.MrRobot.UserName, date);
                    }
                }
                catch (OperationCanceledException)
                {
                    logger.Information($"Timed out when calculating optimal wikipedia path. Made {calculator.TotalRequestsMade} requests. {linksCache.TotalCacheHits} cache hits.", null);
                }
                catch (Exception e)
                {
                    logger.Log(LogRowSeverity.Error, $"Error when calculating optimal wikipedia path: {e.Message}", null, e.StackTrace);
                }
                finally
                {
                    linksCache.SaveCacheToDatabase();
                }
            }).Start();
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
            string language;
            string title;
            if (mostVotes > 0)
            {
                var suggestions = allSuggestions.Where(x => x.Votes == mostVotes).ToList();
                var index = Random.Shared.Next(suggestions.Count);
                var suggestion = suggestions[index];
                suggestion.Voters = [];
                suggestion.Votes = 0;

                language = suggestion.Language;
                title = suggestion.Title;
            }
            else
            {
                language = "en";
                title = GetRandomGoalTitle(language, 10, 100);
            }

            var pageInfo = WikipediaClient.GetPageInfo(title, language);
            return (pageInfo.Title, language, pageInfo.Summary);
        }

        private WikiGameGoalDifficulty GetTitleDifficulty(string title, string language)
        {
            long pageViews = 0;
            try
            {
                pageViews = WikipediaClient.GetPageViews(title, language);
            }
            catch (Exception e)
            {
                logger.Error($"Error when getting page views for page {title}. {e.Message}", null);
                return WikiGameGoalDifficulty.Unknown;
            }
           
            return pageViews switch
            {
                < 5000 => WikiGameGoalDifficulty.Extreme,
                < 20000 => WikiGameGoalDifficulty.Hard,
                < 80000 => WikiGameGoalDifficulty.Normal,
                _ => WikiGameGoalDifficulty.Easy
            };
        }

        private string GetRandomGoalTitle(string language, int minimumRequests, int maximumRequests)
        {
            var totalRequests = 0;
            var bestTitle = "";
            long maxViews = 0;
            var bestLinksCount = 0;

            while (maxViews < MinimumGoalPageViews || totalRequests < minimumRequests || bestLinksCount < MinimumGoalPageLinks)
            {
                var title = WikipediaClient.GetRandomPage(language).Title;

                long pageViews = 0;
                try
                {
                    pageViews = WikipediaClient.GetPageViews(title, language);
                } 
                catch (Exception e)
                {
                    logger.Error($"Error when getting page views for page {title}. {e.Message}", null);
                }
                
                totalRequests++;
                if (pageViews > maxViews)
                {
                    var linkCount = WikipediaClient.GetIncomingLinksToPage(null, title, language, out var _).Count;
                    maxViews = pageViews;
                    bestLinksCount = linkCount;
                    bestTitle = title;
                }

                if (totalRequests > maximumRequests)
                {
                    break;
                }
            }

            return bestTitle;
        }
    }
}
