using SharedModels;
using EasyNetQ;
namespace NewsletterService.Services
{
    public class ArticleCreatedConsumer : BackgroundService
    {
        private readonly IBus _bus;
        public ArticleCreatedConsumer(IBus bus)
        {
            _bus = bus;
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
            // Simulate some publishing work
            Console.WriteLine(message.ToString());
            Console.WriteLine("NewsletterService received ArticleCreatedEvent");
            await Task.CompletedTask;
        }
    }
}
