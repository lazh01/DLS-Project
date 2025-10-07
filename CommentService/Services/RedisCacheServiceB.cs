namespace CommentService.Services
{
    using StackExchange.Redis;
    using System.Text.Json;

    public class CommentCacheService
    {
        private readonly IDatabase _cache;
        private const string ArticleLruKey = "articles:lru";
        private const int MaxCachedArticles = 30;

        public CommentCacheService(string redisConnection)
        {
            var redis = ConnectionMultiplexer.Connect(redisConnection);
            _cache = redis.GetDatabase();
        }

        public async Task AddCommentToCache(Comment comment)
        {
            var commentKey = $"comment:{comment.Id}";
            var articleKey = $"continent:{comment.ArticleContinent}article:{comment.ArticleId}:comments";
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            bool articleExists = await _cache.KeyExistsAsync(articleKey);

            if (!articleExists)
            {
                var count = await _cache.SortedSetLengthAsync(ArticleLruKey);
                if (count >= MaxCachedArticles)
                {
                    var toRemove = await _cache.SortedSetRangeByRankAsync(ArticleLruKey, 0, 0);
                    foreach (var lruArticleKey in toRemove)
                    {
                        await RemoveArticleKey((RedisKey)(string)lruArticleKey);
                    }
                }
            }

            

            var json = JsonSerializer.Serialize(comment);

            await _cache.StringSetAsync(commentKey, json);
            await _cache.SortedSetAddAsync(articleKey, comment.Id.ToString(), timestamp);
            await _cache.SortedSetAddAsync(ArticleLruKey, articleKey, timestamp);
        }

        public async Task<List<Comment>?> GetCommentsByArticleAndContinent(long article_id, string continent)
        {
            var articleKey = $"continent:{continent}article:{article_id}:comments";
            var commentIds = await _cache.SortedSetRangeByRankAsync(articleKey);
            if (commentIds.Length == 0)
                return null;

            var commentKeys = commentIds
                .Select(id => (RedisKey)$"comment:{id}")
                .ToArray();

            var values = await _cache.StringGetAsync(commentKeys);

            var comments = values
                .Where(v => !v.IsNullOrEmpty)
                .Select(v => JsonSerializer.Deserialize<Comment>(v!)!)
                .ToList();

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await _cache.SortedSetAddAsync(ArticleLruKey, articleKey, timestamp);

            return comments;
        }

        public async Task RemoveCommentFromCache(long articleId, string continent, long commentId)
        {
            var articleKey = $"continent:{continent}article:{articleId}:comments";
            var commentKey = $"comment:{commentId}";

            await _cache.KeyDeleteAsync(commentKey);
            await _cache.SortedSetRemoveAsync(articleKey, commentId.ToString());
        }

        public async Task UpdateCommentInCache(Comment comment)
        {
            var commentKey = $"comment:{comment.Id}";
            var json = JsonSerializer.Serialize(comment);
            await _cache.StringSetAsync(commentKey, json);
        }

        public async Task RemoveArticleFromCache(long articleId, string continent)
        {
            var articleKey = $"continent:{continent}article:{articleId}:comments";
            await RemoveArticleKey(articleKey);
        }

        public async Task RemoveArticleKey(string articleKey)
        {
            if (!await _cache.KeyExistsAsync(articleKey))
                return;

            var commentIds = await _cache.SortedSetRangeByRankAsync(articleKey);
            if (commentIds.Length > 0)
            {
                var commentKeys = commentIds
                    .Select(id => (RedisKey)$"comment:{id}")
                    .ToArray();
                await _cache.KeyDeleteAsync(commentKeys);
            }
            await _cache.KeyDeleteAsync(articleKey);
            await _cache.SortedSetRemoveAsync(ArticleLruKey, articleKey);
        }
    }
}