using Articleservice.Services;
using ArticleService;
using ArticleService.database;
namespace Articleservice.Services
{
    public class CacheUpdaterService
    {
        private readonly RedisCacheService _cacheService;
        private readonly Database _database;
        private readonly Coordinator coordinator = new Coordinator();

        private DateTime _lastCachedPublishedAt;

        public CacheUpdaterService(RedisCacheService cacheService, Database database)
        {
            _cacheService = cacheService;
            _database = database;
            _lastCachedPublishedAt = DateTime.UtcNow.AddDays(-14).Date;
        }

        public async Task UpdateCacheAsync()
        {
            var now = DateTime.UtcNow;
            var startWindow = _lastCachedPublishedAt;
            var endWindow = now;

            Console.WriteLine("Fetching articles from DB...");
            var newArticles = await FetchArticlesFromDb(startWindow, endWindow);

            Console.WriteLine("Updating Redis cache...");
            if (newArticles.Count == 0)
            {
                Console.WriteLine("No new articles to cache.");
                return;
            }
            foreach (var article in newArticles)
            {
                // Cache by ID
                await _cacheService.SetArticleByIdAsync(article);

                // Cache by PublishedAt date
                await _cacheService.AppendArticleToDateAsync(article);
            }
            _lastCachedPublishedAt = newArticles.Max(a => a.PublishedAt).AddMilliseconds(1);
            Console.WriteLine("Cache update completed!");
        }
        private async Task<List<Article>> FetchArticlesFromDb(DateTime start, DateTime end)
        {
            var connection = await coordinator.GetConnectionByRegion("global");
            var result = await _database.SelectSqlAsync(connection, $"""
            select * from Articles
            where PublishedAt > '{start:yyyy-MM-dd HH:mm:ss.fff}' and PublishedAt < '{end.AddDays(1):yyyy-MM-dd HH:mm:ss}'
            order by PublishedAt desc
        """);
            if (result is not List<Dictionary<string, object>> rows)
            {
                return new List<Article>();
            }
            var articles = rows.Select(row => new Article
            {
                Id = (long)row["Id"],
                Title = (string)row["Title"],
                Content = (string)row["Content"],
                Author = (string)row["Author"],
                PublishedAt = (DateTime)row["PublishedAt"],
                CreatedAt = (DateTime)row["CreatedAt"],
                Continent = (string)row["Continent"]
            }).ToList();

            return articles;
        }
    }
}
