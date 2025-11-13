using SharedModels;
using EasyNetQ;
using Monitoring;
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
            using (var activity = MonitorService.ActivitySource.StartActivity())
            {

                if (activity != null)
                {
                    // Inject current trace context into message
                    request.TraceParent = activity.Id;
                    request.TraceState = activity.TraceStateString;
                }


                MonitorService.Log.Information("Publishing article: {Title} by {Author}", request.Title, request.Author);
                await _bus.PubSub.PublishAsync(request);
                MonitorService.Log.Information("Published article: {Title} by {Author}", request.Title, request.Author);
            }
            return "Publishing completed successfully.";
        }
    }
}
