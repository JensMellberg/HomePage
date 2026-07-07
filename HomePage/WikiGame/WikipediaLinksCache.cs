using HomePage.Data;
using HomePage.Model;
using Microsoft.EntityFrameworkCore;

namespace HomePage.WikiGame
{
    public class WikipediaLinksCache(AppDbContext dbContext)
    {
        private Dictionary<long, CachedWikiGameLinks> CacheToSave { get; set; } = [];

        private Dictionary<long, CachedWikiGameLinks> LocalCache { get; set; } = [];

        private const int DaysToCache = 5;

        public long TotalCacheHits { get; private set; } = 0;

        public IEnumerable<long> GetOutgoingLinks(long pageId, string language, out long requestsMade)
            => GetLinks(
                pageId,
                language,
                out requestsMade,
                cache => cache.OutgoingLinks,
                (cache, links) => cache.OutgoingLinks = links,
                WikipediaClient.GetOutgoingLinksFromPage);

        public IEnumerable<long> GetIncomingLinks(long pageId, string language, out long requestsMade)
            => GetLinks(
                pageId,
                language,
                out requestsMade,
                cache => cache.IncomingLinks,
                (cache, links) => cache.IncomingLinks = links,
                WikipediaClient.GetIncomingLinksToPage);

        public void SaveCacheToDatabase()
        {
            if (CacheToSave.Count == 0)
            {
                return;
            }

            var ids = CacheToSave.Keys.ToHashSet();

            var existing = dbContext.CachedWikiGameLinks
                .Where(x => ids.Contains(x.PageId))
                .ToDictionary(x => x.PageId);

            var newEntries = new List<CachedWikiGameLinks>();

            foreach (var (pageId, cached) in CacheToSave)
            {
                if (existing.TryGetValue(pageId, out var entity))
                {
                    entity.CacheDate = cached.CacheDate;

                    if (cached.OutgoingLinks != null)
                    {
                        entity.OutgoingLinks = cached.OutgoingLinks;
                    }

                    if (cached.IncomingLinks != null)
                    {
                        entity.IncomingLinks = cached.IncomingLinks;
                    }
                }
                else
                {
                    newEntries.Add(cached);
                }
            }

            if (newEntries.Count > 0)
            {
                dbContext.CachedWikiGameLinks.AddRange(newEntries);
            } 

            dbContext.SaveChanges();
            CacheToSave.Clear();
        }

        public void CleanStaleEntries()
        {
            var cutOffDate = DateHelper.DateNow.AddDays(-DaysToCache);
            dbContext.CachedWikiGameLinks.Where(x => x.CacheDate < cutOffDate).ExecuteDelete();
        }

        private IEnumerable<long> GetLinks(
            long pageId,
            string language,
            out long requestsMade,
            Func<CachedWikiGameLinks, IEnumerable<long>?> getCachedLinks,
            Action<CachedWikiGameLinks, List<long>> setCachedLinks,
            LinkFetcher fetchLinks)
        {
            requestsMade = 0;

            if (LocalCache.TryGetValue(pageId, out var localCachedPage))
            {
                var cachedLinks = getCachedLinks(localCachedPage);
                if (cachedLinks != null)
                {
                    TotalCacheHits++;
                    return cachedLinks;
                }
            }

            var dbCache = dbContext.CachedWikiGameLinks.Find(pageId);
            if (dbCache != null)
            {
                var cachedLinks = getCachedLinks(dbCache);
                if (cachedLinks != null)
                {
                    TotalCacheHits++;
                    dbCache.CacheDate = DateHelper.DateTimeNow;
                    StoreInDictionary(LocalCache, dbCache);
                    return cachedLinks;
                }
            }

            var links = fetchLinks(pageId, null, language, out requestsMade);

            var linksToCache = links
                .Select(x => x.Id)
                .ToList();

            var cacheObject = dbCache ?? new CachedWikiGameLinks
            {
                PageId = pageId,
                CacheDate = DateHelper.DateTimeNow
            };

            setCachedLinks(cacheObject, linksToCache);

            StoreInDictionary(LocalCache, cacheObject);
            StoreInDictionary(CacheToSave, cacheObject);

            return linksToCache;
        }

        private delegate List<WikipediaPageMetadata> LinkFetcher(
            long? pageId,
            string title,
            string language,
            out long requestsMade);

        private void StoreInDictionary(Dictionary<long, CachedWikiGameLinks> dict, CachedWikiGameLinks updated)
        {
            if (dict.TryGetValue(updated.PageId, out var existing))
            {
                if (updated.OutgoingLinks != null)
                {
                    existing.OutgoingLinks = updated.OutgoingLinks;
                }  

                if (updated.IncomingLinks != null)
                {
                    existing.IncomingLinks = updated.IncomingLinks;
                }  

                existing.CacheDate = updated.CacheDate;
            }
            else
            {
                dict.Add(updated.PageId, updated);
            }
        }
    }
}
