using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Messages.SharedModels;
using NewsletterService.Services;

namespace NewsletterService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsletterController : ControllerBase
    {
        private readonly FetchArticlesService _fetchArticlesService;
        public NewsletterController(FetchArticlesService fetchArticlesService)
        {
            _fetchArticlesService = fetchArticlesService;
        }
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { Message = "Newsletter Service is running." });
        }
        [HttpPost("fetch")]
        public async Task<ActionResult<List<ArticleDTO>>> FetchArticles(FetchArticlesRequest request)
        {
            List<ArticleDTO> articles = await _fetchArticlesService.FetchArticlesAsync(request);

            return Ok(articles);
        }
    }
}
