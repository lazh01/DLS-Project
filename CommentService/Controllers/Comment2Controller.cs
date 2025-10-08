using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CommentService.Services;
using CommentService.Repositories;

namespace CommentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Comment2Controller : ControllerBase
    {
        private readonly ICommentRepository _repository;
        public Comment2Controller(ICommentRepository repository)
        {
            _repository = repository;
        }
        
        private static readonly List<string> continents = new List<string>
        {
            "africa",
            "antarctica",
            "asia",
            "europe",
            "north america",
            "oceania",
            "south america"
        };

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateCommentRequest com)
        {
            if (com.ArticleId <= 0)
            {
                return BadRequest(new { message = "Invalid ArticleId. It must be a positive number." });
            }
            else if (string.IsNullOrWhiteSpace(com.Username) || com.Username.Length > 100)
            {
                return BadRequest(new { message = "Author is required and must be less than 100 characters." });
            }
            else if (string.IsNullOrWhiteSpace(com.TextContent))
            {
                return BadRequest(new { message = "TextContent is required." });
            }
            else if (string.IsNullOrWhiteSpace(com.ArticleContinent) || !continents.Contains(com.ArticleContinent))
            {
                return BadRequest(new { message = "Continent is required and must be one of the following: " + string.Join(", ", continents) });
            }
            try
            {
                var createdComment = await _repository.AddComment(com);
                return CreatedAtAction(nameof(Post), new { id = createdComment.Id }, createdComment);
            }
            catch (ProfanityDetectedException)
            {
                return BadRequest(new { message = "Comment contains profanity and cannot be added." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing your request." });
            }
        }

        [HttpGet("FetchComments")]
        public async Task<IActionResult> FetchComments([FromQuery] FetchCommentsRequest request)
        {
            if (request.ArticleId <= 0)
            {
                return BadRequest(new { message = "Invalid ArticleId. It must be a positive number." });
            }
            else if (string.IsNullOrWhiteSpace(request.ArticleContinent) || !continents.Contains(request.ArticleContinent))
            {
                return BadRequest(new { message = "Continent is required and must be one of the following: " + string.Join(", ", continents) });
            }
            try
            {
                var comments = await _repository.GetCommentsByArticleAndContinent(request.ArticleId, request.ArticleContinent);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing your request." });
            }
        }
    }
}
