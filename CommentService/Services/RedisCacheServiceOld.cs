using StackExchange.Redis;
using System.Text.Json;
namespace CommentService.Services
{
    public class RedisCacheServiceOld
    {
        private readonly IDatabase _cache;
        private const string LruIndexKey = "articles:lru";
        private const int MaxArticles = 30;
        public RedisCacheServiceOld(string redisConnection)
        {
            var redis = ConnectionMultiplexer.Connect(redisConnection);
            _cache = redis.GetDatabase();
        }

        public async Task SetCommentsByArticleAndContinent(long articleId, string continent, List<Comment> comments)
        {
            // Evict least recently used articles if > MaxArticles
            var count = await _cache.SortedSetLengthAsync(LruIndexKey);
            if (count == MaxArticles)
            {
                // Get the lowest scored (least recently used) entries
                var toRemove = await _cache.SortedSetRangeByRankAsync(LruIndexKey, 0, (long)(count - MaxArticles));
                foreach (var oldKey in toRemove)
                {
                    await _cache.KeyDeleteAsync((RedisKey)(string)oldKey);
                    await _cache.SortedSetRemoveAsync(LruIndexKey, oldKey);
                }
            }

            var key = $"article:{articleId}:continent:{continent}";
            var json = JsonSerializer.Serialize(comments);
            await _cache.StringSetAsync(key, json);

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await _cache.SortedSetAddAsync(LruIndexKey, key, timestamp);

        }

        public async Task<List<Comment>?> GetCommentsByArticleAndContinent(long articleId, string continent)
        {
            var key = $"article:{articleId}:continent:{continent}";
            var json = await _cache.StringGetAsync(key);
            if (json.IsNullOrEmpty)
                return null;

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await _cache.SortedSetAddAsync(LruIndexKey, key, timestamp);
            return JsonSerializer.Deserialize<List<Comment>>(json!);
        }

        public async Task InvalidateCommentsCache(long articleId, string continent)
        {
            var key = $"article:{articleId}:continent:{continent}";
            await _cache.KeyDeleteAsync(key);
        }
    }
}
