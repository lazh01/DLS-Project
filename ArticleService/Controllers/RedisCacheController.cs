using Articleservice.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Articleservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedisCacheController : ControllerBase
    {
        private readonly CacheUpdaterService _cacheUpdater;
        public RedisCacheController(CacheUpdaterService cacheUpdater)
        {
            _cacheUpdater = cacheUpdater;
        }

        [HttpGet]
        public async Task<IActionResult> Update()
        {
            try 
            {
                await _cacheUpdater.UpdateCacheAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Failed to start cache updater: {ex.Message}" });
            }
            return Ok(new { Message = "Redis Cache Updated." });
        }
    }
}
