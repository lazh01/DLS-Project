using StackExchange.Redis;
using System.Text.Json;
using Monitoring;

namespace Articleservice.Services
{
    public class RedisCacheService
    {
        private readonly IDatabase _cache;
        public RedisCacheService(string redisConnection)
        {
            var redis = ConnectionMultiplexer.Connect(redisConnection);
            _cache = redis.GetDatabase();
        }

        public async Task<Article?> GetArticleByIdAsync(long id)
        {
            string key = $"article:global:{id}";
            var value = await _cache.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                MonitorService.Log.Information(
                    "Global Article CACHE MISS: Article {ArticleId}",
                    id);
                return null;
            }
            else
            {
                MonitorService.Log.Information(
                    "Global Article CACHE HIT: Article {ArticleId}",
                    id);
                return JsonSerializer.Deserialize<Article>(value);
            }
        }

        public async Task SetArticleByIdAsync(Article article, TimeSpan? ttl = null)
        {
            string key = $"article:global:{article.Id}";
            var serialized = JsonSerializer.Serialize(article);

            TimeSpan expiration;
            if (ttl.HasValue)
            {
                expiration = ttl.Value;
            }
            else
            {
                // Default: expire 15 days after PublishedAt to cover the rolling window
                expiration = article.PublishedAt.Date.AddDays(15) - DateTime.UtcNow;
                if (expiration <= TimeSpan.Zero)
                    expiration = TimeSpan.FromSeconds(1); // expire immediately if old
            }

            await _cache.StringSetAsync(key, serialized, expiration);
        }

        public async Task<List<Article>?> GetArticlesByDateAsync(DateOnly date)
        {
            string key = $"articles:global:{date:yyyy-MM-dd}";
            var value = await _cache.StringGetAsync(key);
            if (value.IsNullOrEmpty) return null;
            return JsonSerializer.Deserialize<List<Article>>(value);
        }

        public async Task SetArticlesByDateAsync(DateOnly date, List<Article> articles, TimeSpan ttl)
        {
            string key = $"articles:global:{date:yyyy-MM-dd}";
            var serialized = JsonSerializer.Serialize(articles);
            await _cache.StringSetAsync(key, serialized, ttl);
        }

        public async Task AppendArticleToDateAsync(Article article, TimeSpan? ttl = null)
        {
            var date = DateOnly.FromDateTime(article.PublishedAt);
            var existing = await GetArticlesByDateAsync(date) ?? new List<Article>();
            existing.Add(article);
            TimeSpan expiration;
            if (ttl.HasValue)
            {
                expiration = ttl.Value;
            }
            else
            {
                // Default: expire 15 days after PublishedAt to cover the rolling window
                expiration = article.PublishedAt.Date.AddDays(15) - DateTime.UtcNow;
                if (expiration <= TimeSpan.Zero)
                    expiration = TimeSpan.FromSeconds(1); // expire immediately if old
            }
            await SetArticlesByDateAsync(date, existing, expiration);
        }

        public async Task<List<Article>> GetArticlesInRangeAsync(DateOnly start, DateOnly end)
        {
            var results = new List<Article>();
            for (var date = start; date <= end; date = date.AddDays(1))
            {
                var articles = await GetArticlesByDateAsync(date);
                if (articles != null) results.AddRange(articles);
            }
            return results;
        }
    }
}
