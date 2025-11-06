using Microsoft.AspNetCore.Mvc;
using SubscriberService.DTOs;
using SubscriberService.Services;
using System.Threading.Tasks;

namespace SubscriberService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscribersController : ControllerBase
    {
        private readonly SubscriptionService _subscriberService;

        public SubscribersController(SubscriptionService subscriberService)
        {
            _subscriberService = subscriberService;
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
                return BadRequest("Username is required.");

            try
            {
                var success = await _subscriberService.SubscribeAsync(dto.Username);
                if (!success)
                    return BadRequest("User is already subscribed.");

                return Ok($"User '{dto.Username}' subscribed successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, ex.Message); // Service disabled
            }
        }

        [HttpPost("unsubscribe")]
        public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
                return BadRequest("Username is required.");

            try
            {
                var success = await _subscriberService.UnsubscribeAsync(dto.Username);
                if (!success)
                    return BadRequest("User is not currently subscribed.");

                return Ok($"User '{dto.Username}' unsubscribed successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, ex.Message); // Service disabled
            }
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetStatus(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest("Username is required.");

            try
            {
                var isSubscribed = await _subscriberService.IsSubscribedAsync(username);
                return Ok(new { Username = username, IsSubscribed = isSubscribed });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, ex.Message); // Service disabled
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSubscribers()
        {
            try
            {
                var subscribers = await _subscriberService.GetAllSubscribersAsync();
                return Ok(subscribers);
            }
            catch (InvalidOperationException ex)
            {
                // Service disabled via FeatureHub
                return StatusCode(503, ex.Message);
            }
        }
    }
}