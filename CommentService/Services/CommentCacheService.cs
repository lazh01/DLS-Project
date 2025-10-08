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
            Console.WriteLine($"Adding comment {comment.Id} to cache.");
            var commentKey = $"comment:{comment.Id}";
            var articleKey = $"continent:{comment.ArticleContinent}article:{comment.ArticleId}:comments";
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            bool articleExists = await _cache.KeyExistsAsync(articleKey);

            if (!articleExists)
            {
                Console.WriteLine($"Article key {articleKey} does not exist in cache. Checking LRU cache size.");
                var count = await _cache.SortedSetLengthAsync(ArticleLruKey);
                if (count >= MaxCachedArticles)
                {
                    var toRemove = await _cache.SortedSetRangeByRankAsync(ArticleLruKey, 0, 0);
                    Console.WriteLine($"Cache limit reached. Evicting least recently used article key(s): {string.Join(", ", toRemove.Select(k => (string)k))}");
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
            Console.WriteLine($"Comment {comment.Id} added to cache under article key {articleKey}.");
        }

        public async Task<List<Comment>?> GetCommentsByArticleAndContinent(long article_id, string continent)
        {
            Console.WriteLine($"Fetching comments for article {article_id} in continent {continent} from cache.");
            var articleKey = $"continent:{continent}article:{article_id}:comments";
            var commentIds = await _cache.SortedSetRangeByRankAsync(articleKey);
            if (commentIds.Length == 0) 
            {
                Console.WriteLine($"No comments found in cache for article {article_id} in continent {continent}.");
                return null;
            }

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
            Console.WriteLine($"Fetched {comments.Count} comments from cache for article {article_id} in continent {continent}.");
            return comments;
        }

        public async Task RemoveCommentFromCache(long articleId, string continent, long commentId)
        {
            Console.WriteLine($"Removing comment {commentId} from cache.");
            var articleKey = $"continent:{continent}article:{articleId}:comments";
            var commentKey = $"comment:{commentId}";

            await _cache.KeyDeleteAsync(commentKey);
            await _cache.SortedSetRemoveAsync(articleKey, commentId.ToString());
        }

        public async Task RemoveArticleFromCache(long articleId, string continent)
        {
            var articleKey = $"continent:{continent}article:{articleId}:comments";
            await RemoveArticleKey(articleKey);
        }

        private async Task RemoveArticleKey(string articleKey)
        {
            Console.WriteLine($"Removing article key {articleKey} and its comments from cache.");
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