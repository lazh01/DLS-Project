using EasyNetQ;

namespace NewsletterService.Wrappers
{
    public class SubscriberBus
    {
        public IBus Bus { get; }
        public SubscriberBus(IBus bus) => Bus = bus;
    }

    public class ArticleBus
    {
        public IBus Bus { get; }
        public ArticleBus(IBus bus) => Bus = bus;
    }
}
