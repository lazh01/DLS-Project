using EasyNetQ;
using Microsoft.Extensions.Caching.Memory;
using SharedModels;

namespace Articleservice.Services
{
    public class PublishingConsumer : BackgroundService
    {
        private readonly IBus _bus;
        private readonly IMemoryCache _cache;

        public PublishingConsumer(IBus bus, IMemoryCache cache)
        {
            _bus = bus;
            _cache = cache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Subscribe to ArticleCreatedEvent
            await _bus.PubSub.SubscribeAsync<CreateArticleRequest>(
                subscriptionId: "published_article_consumer",  // unique name for consumer group
                onMessage: HandleMessage
            );

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private Task HandleMessage(CreateArticleRequest message)
        {
            /*var key = $"{message.Title}-{message.Author}";

            // Try to add to cache. If it already exists, skip processing
            if (_cache.TryGetValue(key, out _))
                return Task.CompletedTask;

            // Add to cache with expiration (e.g., 5 minutes)
            _cache.Set(key, true, TimeSpan.FromSeconds(30));
            */
            // Process the message (e.g., log it, store it, etc.)
            Console.WriteLine($"Received article: Title={message.Title}, Author={message.Author}, Continent={message.Continent}");
            // Simulate some processing work
            return Task.CompletedTask;
        }
    }
}
