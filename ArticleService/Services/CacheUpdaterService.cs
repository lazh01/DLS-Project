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

        public CacheUpdaterService(RedisCacheService cacheService, Database database)
        {
            _cacheService = cacheService;
            _database = database;
        }

        public async Task UpdateCacheAsync()
        {
            var now = DateTime.UtcNow;
            var startDate = now.AddDays(-14); // last 14 days
            var endDate = now;

            Console.WriteLine("Fetching articles from DB...");
            var articles = await FetchArticlesFromDb(startDate, endDate);

            Console.WriteLine("Updating Redis cache...");
            foreach (var article in articles)
            {
                // Cache by ID
                await _cacheService.SetArticleByIdAsync(article);

                // Cache by PublishedAt date
                await _cacheService.AppendArticleToDateAsync(article);
            }

            Console.WriteLine("Cache update completed!");
        }
        private async Task<List<Article>> FetchArticlesFromDb(DateTime start, DateTime end)
        {
            var connection = await coordinator.GetConnectionByRegion("global");
            var result = await _database.SelectSqlAsync(connection, $"""
            select * from Articles
            where PublishedAt between '{start:yyyy-MM-dd}' and '{end:yyyy-MM-dd}'
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
