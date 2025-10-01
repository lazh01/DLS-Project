using Azure.Core;
using EasyNetQ;
using Microsoft.Extensions.Caching.Memory;
using SharedModels;

using ArticleService;

namespace Articleservice.Services
{
    public class PublishingConsumer : BackgroundService
    {
        private readonly IBus _bus;
        private readonly IMemoryCache _cache;
        private Database database = Database.GetInstance();

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

        private async Task HandleMessage(CreateArticleRequest message)
        {
            /*var key = $"{message.Title}-{message.Author}";

            // Try to add to cache. If it already exists, skip processing
            if (_cache.TryGetValue(key, out _))
                return Task.CompletedTask;

            // Add to cache with expiration (e.g., 5 minutes)
            _cache.Set(key, true, TimeSpan.FromSeconds(30));
            */
            // Process the message (e.g., log it, store it, etc.)
            Database database = Database.GetInstance();

            var article = new Article
            {
                Title = message.Title,
                Content = message.Content,
                Author = message.Author,
                Continent = message.Continent
            };
            Console.WriteLine("Inserting article into database...");
            var newId = await database.InsertArticle(article);
            Console.WriteLine($"Inserted article with ID {newId}.");
            Console.WriteLine($"Received article: Title={message.Title}, Author={message.Author}, Continent={message.Continent}");
            
            return;
        }
    }
}
