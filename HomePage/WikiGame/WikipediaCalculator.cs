using HomePage.Repositories;

namespace HomePage.WikiGame
{
    public class WikipediaCalculator(string language, WikiGameRepository wikiGameRepository, WikipediaLinksCache linksCache)
    {
        public long TotalRequestsMade { get; private set; }

        private const int RestInterval = 1000;
        private const int RestMinutes = 5;

        public List<WikipediaPageMetadata> FindOptimalPath(string fromPage, string toPage, CancellationToken token)
        {
            linksCache.CleanStaleEntries();
            var nextRestPeriod = RestInterval;
            List<WikipediaPageMetadata>? result = null;
            var invalidPages = new HashSet<long>();
            var pageById = new Dictionary<long, WikipediaPageMetadata>();
            var fromPageInfo = WikipediaClient.GetPageInfo(fromPage, language);
            pageById.Add(fromPageInfo.Id, fromPageInfo);

            var toPageInfo = WikipediaClient.GetPageInfo(toPage, language);
            pageById.Add(toPageInfo.Id, toPageInfo);

            var visitedFrom = new Dictionary<long, (int dist, long previous)>();
            var visitedTo = new Dictionary<long, (int dist, long previous)>();

            var fromQueue = new Queue<(WikipediaPageMetadata page, int dist)>();
            var toQueue = new Queue<(WikipediaPageMetadata page, int dist)>();

            EnqueueAllOutgoingLinks(fromPageInfo, fromQueue, 1);
            EnqueueAllIncomingLinks(toPageInfo, toQueue, 1);
            if (result != null)
            {
                return result;
            }

            while (fromQueue.Count > 0 || toQueue.Count > 0)
            {
                if (fromQueue.Count < toQueue.Count)
                {
                    EmptyFromQueue();
                    if (result != null)
                    {
                        return result;
                    }

                    RestIfNeeded();
                    EmptyToQueue();
                }
                else
                {
                    EmptyToQueue();
                    if (result != null)
                    {
                        return result;
                    }

                    RestIfNeeded();
                    EmptyFromQueue();
                }

                if (result != null)
                {
                    return result;
                }

                RestIfNeeded();
            }

            return [];

            void RestIfNeeded()
            {
                if (TotalRequestsMade > nextRestPeriod)
                {
                    nextRestPeriod += RestInterval;
                    Thread.Sleep(TimeSpan.FromMinutes(RestMinutes));
                }
            }

            void EmptyFromQueue()
            {
                if (fromQueue.Count > 0)
                {
                    var tempQueue = new Queue<(WikipediaPageMetadata page, int dist)>();
                    while (fromQueue.Count > 0)
                    {
                        var (current, dist) = fromQueue.Dequeue();
                        EnqueueAllOutgoingLinks(current, tempQueue, dist + 1);
                        if (result != null)
                        {
                            return;
                        }
                    }

                    fromQueue = tempQueue;
                }
            }

            void EmptyToQueue()
            {
                if (toQueue.Count > 0)
                {
                    var tempQueue = new Queue<(WikipediaPageMetadata page, int dist)>();
                    while (toQueue.Count > 0)
                    {
                        var (current, dist) = toQueue.Dequeue();
                        EnqueueAllIncomingLinks(current, tempQueue, dist + 1);
                        if (result != null)
                        {
                            return;
                        }
                    }

                    toQueue = tempQueue;
                }
            }

            void EnqueueAllOutgoingLinks(WikipediaPageMetadata page, Queue<(WikipediaPageMetadata, int)> queue, int dist)
            {
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                var outGoingLinks = linksCache.GetOutgoingLinks(page.Id, page.Title, language, out var localRequests);
                TotalRequestsMade += localRequests;
                foreach (var link in outGoingLinks)
                {
                    if (visitedFrom.ContainsKey(link.Id))
                    {
                        continue;
                    }

                    queue.Enqueue((link, dist));
                    pageById.TryAdd(link.Id, link);
                    visitedFrom.Add(link.Id, (dist, page.Id));

                    if (visitedTo.ContainsKey(link.Id) || link.Id == toPageInfo.Id)
                    {
                        OnMatchFound(page.Id, link.Id);
                        if (result != null)
                        {
                            return;
                        }
                    }
                }
            }

            void EnqueueAllIncomingLinks(WikipediaPageMetadata page, Queue<(WikipediaPageMetadata, int)> queue, int dist)
            {
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                var incomingLinks = linksCache.GetIncomingLinks(page.Id, page.Title, language, out var localRequests);
                TotalRequestsMade += localRequests;
                foreach (var link in incomingLinks)
                {
                    if (visitedTo.ContainsKey(link.Id))
                    {
                        continue;
                    }
                    queue.Enqueue((link, dist));
                    pageById.TryAdd(link.Id, link);
                    visitedTo.Add(link.Id, (dist, page.Id));

                    if (visitedFrom.ContainsKey(link.Id))
                    {
                        OnMatchFound(link.Id, page.Id);
                        if (result != null)
                        {
                            return;
                        }
                    }
                }
            }
            
            void OnMatchFound(long fromPageId, long toPageId)
            {
                var bestPath = new List<WikipediaPageMetadata>();
                var backPath = new List<WikipediaPageMetadata>();
                var frontPath = new List<WikipediaPageMetadata>();

                var crntBackPath = fromPageId;
                while (crntBackPath != fromPageInfo.Id)
                {
                    var navigation = visitedFrom[crntBackPath];
                    backPath.Insert(0, pageById[crntBackPath]);
                    crntBackPath = navigation.previous;
                    if (invalidPages.Contains(crntBackPath))
                    {
                        return;
                    }
                }

                var crntFrontPath = toPageId;
                while (crntFrontPath != toPageInfo.Id)
                {
                    var navigation = visitedTo[crntFrontPath];
                    frontPath.Add(pageById[crntFrontPath]);
                    crntFrontPath = navigation.previous;
                    if (invalidPages.Contains(crntFrontPath))
                    {
                        return;
                    }
                }

                backPath.Insert(0, fromPageInfo);
                frontPath.Add(toPageInfo);
                result = backPath.Concat(frontPath).ToList();

                var allowedLinks = wikiGameRepository.GetPageContent(fromPageInfo.Title, language).AllowedLinks;
                for (var i = 1; i < result.Count; i++)
                {
                    var page = result[i];

                    var nextPage = wikiGameRepository.GetPageContent(page.Title, language);
                    if (!allowedLinks.Contains(nextPage.Title.Replace(' ', '_')))
                    {
                        invalidPages.Add(page.Id);
                        result = null;
                        return;
                    }

                    allowedLinks = nextPage.AllowedLinks;
                }
            }
        }
    }
}
