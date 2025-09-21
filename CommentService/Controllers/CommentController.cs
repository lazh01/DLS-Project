using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CommentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {


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
        // GET: api/<CommentController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<CommentController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<CommentController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateCommentRequest com)
        {
            CommentManager mgr = new CommentManager();
            if (com.ArticleId <= 0)
            {
                return BadRequest(new { message = "Invalid ArticleId. It must be a positive number." });
            } else if (string.IsNullOrWhiteSpace(com.Username) || com.Username.Length > 100)
            {
                return BadRequest(new { message = "Author is required and must be less than 100 characters." });
            } else if (string.IsNullOrWhiteSpace(com.TextContent))
            {
                return BadRequest(new { message = "TextContent is required." });
            } else if (string.IsNullOrWhiteSpace(com.ArticleContinent) || !continents.Contains(com.ArticleContinent))
            {
                return BadRequest(new { message = "Continent is required and must be one of the following: " + string.Join(", ", continents) });
            } 
            try
            {
                bool isCreated = await mgr.CreateComment(com);

                if (isCreated)
                {
                    // Comment was clean and created
                    return Ok(new { message = "Comment has been created." });
                }
                else
                {
                    // Comment contained profanity
                    return BadRequest(new { message = "Comment contains profanity and was not created." });
                }
            }
            catch (Exception ex)
            {
                // Service failure or network error
                return StatusCode(503, new { message = $"Could not check comment: {ex.Message}" });
            }

        }
        [HttpGet("fetchArticles")]
        public async Task<IActionResult> FetchArticles([FromQuery] FetchCommentsRequest request)
        {   
            try {
                if (request.ArticleId <= 0)
                {
                    return BadRequest(new { message = "Invalid ArticleId. It must be a positive number." });
                }
                else if (string.IsNullOrWhiteSpace(request.ArticleContinent) || !continents.Contains(request.ArticleContinent))
                {
                    return BadRequest(new { message = "Continent is required and must be one of the following: " + string.Join(", ", continents) });
                }
                var comments = await Database.FetchCommentsAsync(request.ArticleId, request.ArticleContinent);
                return Ok(comments);
            } catch (Exception ex)
            {
                return BadRequest(new { message = $"Invalid request parameters: {ex.Message}" });
            }
            
        }

        // PUT api/<CommentController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CommentController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
