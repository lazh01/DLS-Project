namespace Articleservice.Services
{
    public class CacheUpdateWorker : BackgroundService
    {
        private readonly CacheUpdaterService _cacheUpdater;

        public CacheUpdateWorker(CacheUpdaterService cacheUpdater)
        {
            _cacheUpdater = cacheUpdater;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine("Starting offline cache update...");
                    await _cacheUpdater.UpdateCacheAsync();
                    Console.WriteLine("Offline cache update completed.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error updating cache: " + ex.Message);
                }

                var now = DateTime.UtcNow;
                var nextRun = new DateTime(now.Year, now.Month, now.Day, 2, 0, 0, DateTimeKind.Utc);

                
                if (nextRun <= now)
                    nextRun = nextRun.AddDays(1);

                var delay = nextRun - now;
                Console.WriteLine($"Next cache update scheduled in {delay.TotalMinutes:F1} minutes.");

                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}
