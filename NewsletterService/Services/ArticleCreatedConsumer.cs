using EasyNetQ;
using NewsletterService.Wrappers;
using SharedModels;
using Monitoring;
using System.Diagnostics;
namespace NewsletterService.Services
{
    public class ArticleCreatedConsumer : BackgroundService
    {
        private readonly IBus _bus;
        private readonly SubscriberApiClient _subscriberApi;
        public ArticleCreatedConsumer(ArticleBus busWrapper, SubscriberApiClient subscriberApi)
        {
            _bus = busWrapper.Bus;
            _subscriberApi = subscriberApi;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _bus.PubSub.SubscribeAsync<ArticleCreatedEvent>(
                subscriptionId: "article_created",
                onMessage: ArticleCreatedHandler,
                cancellationToken: stoppingToken
            );
        }

        public async Task ArticleCreatedHandler(ArticleCreatedEvent message)
        {

            ActivityContext parentContext = default;
            if (!string.IsNullOrEmpty(message.TraceParent))
            {
                ActivityContext.TryParse(message.TraceParent, message.TraceState, out parentContext);
            }

            using (var activity = MonitorService.ActivitySource.StartActivity(
                "ArticleCreatedHandler",
                ActivityKind.Consumer,
                parentContext
            ))
            {
                try
                {
                    var subscribers = await _subscriberApi.GetAllSubscribersAsync();
                    var activeSubscribers = subscribers.Where(s => s.IsSubscribed).ToList();

                    MonitorService.Log.Information(
                        "Sending article '{Title}' to {Count} active subscribers.",
                        message.Title, activeSubscribers.Count
                    );

                    // Simulate delivery or further processing
                }
                catch (Exception ex)
                {
                    MonitorService.Log.Error(ex, "Error processing ArticleCreatedEvent for {Title}", message.Title);
                    throw;
                }
            }
        }
    }
}
