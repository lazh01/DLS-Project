using EasyNetQ;
using FeatureHubSDK;
using Microsoft.EntityFrameworkCore;
using SubscriberService.Data;
using SubscriberService.Domain;
using Messages.SharedModels;
using System;
using System.Threading.Tasks;

namespace SubscriberService.Services
{
    public class SubscriptionService
    {
        private readonly SubscriberDbContext _context;
        private readonly IClientContext _fhClient;
        private const string FeatureKey = "enable_subscriber_service";
        private readonly IBus _bus;

        public SubscriptionService(SubscriberDbContext context, IClientContext fhClient, IBus bus)
        {
            _context = context;
            _fhClient = fhClient;
            _bus = bus;
        }

        private void EnsureFeatureEnabled()
        {
            var flag = _fhClient[FeatureKey].IsEnabled;
            if (!flag)
            {
                throw new InvalidOperationException("Subscriber service is currently disabled.");
            }
        }

        public async Task<bool> SubscribeAsync(string username)
        {
            EnsureFeatureEnabled();

            var existing = await _context.Subscribers
                .FirstOrDefaultAsync(x => x.Username == username);

            if (existing != null)
            {
                if (existing.IsSubscribed) return false;

                existing.IsSubscribed = true;
                existing.SubscribedAt = DateTime.UtcNow;
                existing.UnsubscribedAt = null;
            }
            else
            {
                var subscriber = new Subscriber
                {
                    Username = username,
                    IsSubscribed = true
                };
                await _context.Subscribers.AddAsync(subscriber);
            }

            await _context.SaveChangesAsync();

            var message = new SubscriberEvent
            {
                Username = username,
                Timestamp = DateTime.UtcNow,
                EventType = "Subscribed"
            };

            try
            {
                await _bus.PubSub.PublishAsync(message);
            }
            catch
            {
                // Log but do not block the main flow
                Console.WriteLine($"Failed to publish message for user {username}, will retry later if using a background retry mechanism.");
            }


            return true;
        }

        public async Task<bool> UnsubscribeAsync(string username)
        {
            EnsureFeatureEnabled();

            var existing = await _context.Subscribers
                .FirstOrDefaultAsync(x => x.Username == username && x.IsSubscribed);

            if (existing == null) return false;

            existing.IsSubscribed = false;
            existing.UnsubscribedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsSubscribedAsync(string username)
        {
            EnsureFeatureEnabled();

            var existing = await _context.Subscribers
                .FirstOrDefaultAsync(x => x.Username == username);

            return existing != null && existing.IsSubscribed;
        }
    }
}