using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;

namespace SubscriberService.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SubscriberDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SubscriberDbContext>>();

            // Retry policy: 5 attempts with exponential backoff (2s, 4s, 8s, 16s, 32s)
            AsyncRetryPolicy retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (exception, timespan, attempt, _) =>
                    {
                        logger.LogWarning(
                            exception,
                            "Database connection failed. Retrying attempt {Attempt} in {Delay} seconds...",
                            attempt,
                            timespan.TotalSeconds);
                    });

            retryPolicy.ExecuteAsync(async () =>
            {
                logger.LogInformation("Attempting to migrate the database...");
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migration completed successfully.");
            }).GetAwaiter().GetResult();
        }
    }
}