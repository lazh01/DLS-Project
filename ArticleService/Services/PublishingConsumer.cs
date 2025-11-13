using ArticleService;
using ArticleService.database;
using Azure.Core;
using EasyNetQ;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Monitoring;
using Polly;
using SharedModels;
using System.Diagnostics;

namespace Articleservice.Services
{
    public class PublishingConsumer : BackgroundService
    {
        private readonly IBus _bus;
        private readonly IMemoryCache _cache;
        private readonly Database _database;
        private readonly RedisCacheService _redisCache;

        public PublishingConsumer(IBus bus, IMemoryCache cache, Database database, RedisCacheService redisCache)
        {
            _bus = bus;
            _cache = cache;
            _database = database;
            _redisCache = redisCache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            var retry = Policy.Handle<Exception>()
                    .WaitAndRetryForever(_ => TimeSpan.FromSeconds(5));

            await retry.Execute(async () =>
            {
                await _bus.PubSub.SubscribeAsync<CreateArticleRequest>(
                    "published_article_consumer",
                    HandleMessage,
                    cancellationToken: stoppingToken
                );
            });

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task HandleMessage(CreateArticleRequest message)
        {
            if (message == null)
            {
                MonitorService.Log.Warning("Received null message in HandleMessage");
                return;
            }

            ActivityContext parentContext = default;

            if (!string.IsNullOrEmpty(message.TraceParent))
            {
                ActivityContext.TryParse(message.TraceParent, message.TraceState, out parentContext);
            }

            using var activity = MonitorService.ActivitySource.StartActivity(
                "HandleMessage",
                ActivityKind.Consumer,
                parentContext
            );

            try
            {
                var firstTime = await _redisCache.TryAddArticleKeyAsync(message, TimeSpan.FromMinutes(5));
                if (!firstTime)
                {
                    MonitorService.Log.Information(
                        "Duplicate message detected for '{Title}' by '{Author}', skipping processing.",
                        message.Title,
                        message.Author
                    );
                    return;
                }

                MonitorService.Log.Information(
                    "Received published article message: Title='{Title}', Author='{Author}', Continent='{Continent}'",
                    message.Title,
                    message.Author,
                    message.Continent
                );

                var article = new Article
                {
                    Title = message.Title,
                    Content = message.Content,
                    Author = message.Author,
                    Continent = message.Continent
                };

                MonitorService.Log.Debug("Inserting article '{Title}' by '{Author}' into database.", article.Title, article.Author);

                long? newId = await _database.InsertArticle(article);

                if (newId.HasValue)
                {
                    MonitorService.Log.Information("Successfully inserted article with ID {Id}.", newId);
                }
                else
                {
                    MonitorService.Log.Warning("Article insertion returned null ID for '{Title}' by '{Author}'.", article.Title, article.Author);
                }
            }
            catch (SqlException ex)
            {
                MonitorService.Log.Error(ex, "SQL error while handling message for '{Title}' by '{Author}'", message.Title, message.Author);
                throw; // Rethrow to allow retry or DLQ (Dead Letter Queue) handling
            }
            catch (TimeoutException ex)
            {
                MonitorService.Log.Error(ex, "Timeout while handling message for '{Title}' by '{Author}'", message.Title, message.Author);
                throw;
            }
            catch (Exception ex)
            {
                MonitorService.Log.Error(ex, "Unexpected error while handling message for '{Title}' by '{Author}'", message.Title, message.Author);
                throw;
            }
            finally
            {
                MonitorService.Log.Debug("Finished processing message for '{Title}' by '{Author}'", message.Title, message.Author);
                activity?.Stop();
            }
        }
    }
}
