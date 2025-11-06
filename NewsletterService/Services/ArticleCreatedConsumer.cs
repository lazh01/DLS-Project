using EasyNetQ;
using NewsletterService.Wrappers;
using SharedModels;
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
            var subscribers = await _subscriberApi.GetAllSubscribersAsync();
            var activeSubscribers = subscribers.Where(s => s.IsSubscribed).ToList();

            Console.WriteLine($"Sending article '{message.Title}' to {activeSubscribers.Count} subscribers.");
        }
    }
}
