using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using ArticleService.SharedModels;
using ArticleService.database;
using Messages.SharedModels;
using Articleservice.Models;

namespace ArticleService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticleController : ControllerBase
    {
        private readonly Database _database;

        public ArticleController(Database database)
        {
            _database = database;
        }

        [HttpGet("{continent}/{id:long}", Name = "GetArticle")]
        public async Task<ActionResult<ArticleDTO>> GetArticle(string continent, long id)
        {
            var article = await _database.FindArticle(id, continent);

            if (article == null)
            {
                return NotFound(new { Message = $"Article with ID {id} in {continent} not found." });
            }
            ArticleDTO articledto = ArticleConverter.ToDTO(article);
            return Ok(articledto);
        }

        [HttpPost]
        public async Task<ActionResult<ArticleDTO>> CreateArticle(CreateArticleRequest request)
        {
            var article = new Article
            {
                Title = request.Title,
                Content = request.Content,
                Author = request.Author,
                Continent = request.Continent
            };

            var newId = await _database.InsertArticle(article);
            if (newId == null)
            {
                return StatusCode(500, new { Message = "Failed to create article." });
            }

            var id = newId.Value;
            Console.WriteLine($"Created article with ID {id}.");
            return CreatedAtRoute(
                "GetArticle",
                new { continent = article.Continent, id = id },
                await _database.FindArticle(id, article.Continent)
            );
        }

        [HttpDelete("{continent}/{id:long}")]
        public async Task<ActionResult> Delete(long id, string continent)
        {
            var affected = await _database.DeleteArticle(id, continent);

            if (affected == 0)
            {
                return NotFound(new { Message = $"Article with ID {id} not found in {continent}." });
            }

            return NoContent();
        }

        [HttpPut]
        public async Task<ActionResult> UpdateArticle(ArticleDTO article_dto)
        {
            Article article = ArticleConverter.FromDTO(article_dto);
            var affected = await _database.UpdateArticle(article);
            if (affected == 0)
            {
                return NotFound(new { Message = $"Article with ID {article.Id} not found in {article.Continent}." });
            }
            return NoContent();
        }

        [HttpPost("fetch")]
        public async Task<ActionResult<List<ArticleDTO>>> FetchArticles(FetchArticlesRequest request)
        {
            if (string.IsNullOrEmpty(request.Continent))
            {
                return BadRequest(new { Message = "Continent is required." });
            }
            List<Article> articles = await _database.FetchArticles(request);
            List<ArticleDTO> articlesDTO = articles
                .Select(ArticleConverter.ToDTO)
                .ToList();
            return Ok(articlesDTO);
        }
    }
}