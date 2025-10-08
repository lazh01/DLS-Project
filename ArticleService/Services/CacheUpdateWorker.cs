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

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
