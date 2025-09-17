using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace ArticleService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticleController : ControllerBase
    {
        private Database database = Database.GetInstance();

        [HttpGet("{continent}/{id:long}", Name = "GetArticle")]
        public async Task<ActionResult<Article>> GetArticle(string continent, long id)
        {
            var article = await database.FindArticle(id, continent);

            if (article == null)
            {
                return NotFound(new { Message = $"Article with ID {id} in {continent} not found." });
            }
            return Ok(article);
        }

        [HttpPost]
        public async Task<ActionResult<Article>> CreateArticle(CreateArticleRequest request)
        {
            var article = new Article
            {
                Title = request.Title,
                Content = request.Content,
                Author = request.Author,
                Continent = request.Continent
            };

            var newId = await database.InsertArticle(article);
            if (newId == null)
            {
                return StatusCode(500, new { Message = "Failed to create article." });
            }

            var id = newId.Value;
            Console.WriteLine($"Created article with ID {id}.");
            return CreatedAtRoute(
                "GetArticle",
                new { continent = article.Continent, id = id },
                await database.FindArticle(id, article.Continent)
            );
        }

        [HttpDelete("{continent}/{id:long}")]
        public async Task<ActionResult> Delete(long id, string continent)
        {
            var affected = await database.DeleteArticle(id, continent);

            if (affected == 0)
            {
                return NotFound(new { Message = $"Article with ID {id} not found in {continent}." });
            }

            return NoContent();
        }

        [HttpPut]
        public async Task<ActionResult> UpdateArticle(Article article)
        {
            var affected = await database.UpdateArticle(article);
            if (affected == 0)
            {
                return NotFound(new { Message = $"Article with ID {article.Id} not found in {article.Continent}." });
            }
            return NoContent();
        }
    }
}