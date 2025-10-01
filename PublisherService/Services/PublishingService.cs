using SharedModels;
using EasyNetQ;
namespace PublisherService.Services
{
    public class PublishingService
    {
        private readonly IBus _bus;
        public PublishingService(IBus bus)
        {   
            _bus = bus; 
        }

        public async Task<string> PublishAsync(CreateArticleRequest request)
        {
            // Simulate some publishing work
            await Task.Delay(1000);
            await _bus.PubSub.PublishAsync(request);
            return "Publishing completed successfully.";
        }
    }
}
