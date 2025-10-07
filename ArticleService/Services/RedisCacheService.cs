using StackExchange.Redis;
using System.Text.Json;
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
            if (value.IsNullOrEmpty) return null;
            return JsonSerializer.Deserialize<Article>(value);
        }

        public async Task SetArticleByIdAsync(Article article)
        {
            string key = $"article:global:{article.Id}";
            var serialized = JsonSerializer.Serialize(article);
            await _cache.StringSetAsync(key, serialized, TimeSpan.FromDays(1));
        }

        public async Task<List<Article>?> GetArticlesByDateAsync(DateOnly date)
        {
            string key = $"articles:global:{date:yyyy-MM-dd}";
            var value = await _cache.StringGetAsync(key);
            if (value.IsNullOrEmpty) return null;
            return JsonSerializer.Deserialize<List<Article>>(value);
        }

        public async Task SetArticlesByDateAsync(DateOnly date, List<Article> articles)
        {
            string key = $"articles:global:{date:yyyy-MM-dd}";
            var serialized = JsonSerializer.Serialize(articles);
            await _cache.StringSetAsync(key, serialized, TimeSpan.FromDays(1));
        }

        public async Task AppendArticleToDateAsync(Article article)
        {
            var date = DateOnly.FromDateTime(article.PublishedAt);
            var existing = await GetArticlesByDateAsync(date) ?? new List<Article>();
            existing.Add(article);
            await SetArticlesByDateAsync(date, existing);
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
