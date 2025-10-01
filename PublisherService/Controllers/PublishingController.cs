using Microsoft.AspNetCore.Mvc;
using SharedModels;
using PublisherService.Services;
namespace PublisherService.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class PublishingController : ControllerBase
    {

        private readonly PublishingService _publishingService;

        public PublishingController(PublishingService publishingService)
        {
            _publishingService = publishingService;
        }

        [HttpPost]
        public async Task<IActionResult> Publish( CreateArticleRequest request)
        {
            // Simulate some publishing work
            try 
            {
                var result = await _publishingService.PublishAsync(request);
                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while publishing the article.", Details = ex.Message });
            }
        }
    }
}
