using EasyNetQ;

namespace NewsletterService.Interfaces
{
    public interface ISubscriberBus : IBus { }
    public interface IArticleBus : IBus { }
}
