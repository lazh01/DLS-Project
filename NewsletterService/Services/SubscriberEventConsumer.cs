using EasyNetQ;
using Messages.SharedModels;
using Microsoft.Extensions.Hosting;
using NewsletterService.Wrappers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NewsletterService.Services
{
    public class SubscriberEventConsumer : BackgroundService
    {
        private readonly IBus _bus;
        public SubscriberEventConsumer(SubscriberBus busWrapper)
        {
            _bus = busWrapper.Bus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Subscribe to the queue
            await _bus.PubSub.SubscribeAsync<SubscriberEvent>(
                subscriptionId: "newsletter_subscriber",
                onMessage: HandleSubscriberEvent,
                cancellationToken: stoppingToken
            );
        }

        private Task HandleSubscriberEvent(SubscriberEvent message)
        {
            if (message.EventType == "Subscribed")
            {
                Console.WriteLine($"Welcome {message.Username}! Thanks for subscribing!");
            }
            else if (message.EventType == "Unsubscribed")
            {
                Console.WriteLine($"Goodbye {message.Username}, sorry to see you go!");
            }

            return Task.CompletedTask;
        }
    }
}