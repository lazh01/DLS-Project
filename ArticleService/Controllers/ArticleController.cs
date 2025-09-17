using Microsoft.AspNetCore.Mvc;

namespace ArticleService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticleController : ControllerBase
    {
        private Database database = Database.GetInstance();
        // GET: api/Article/{id}
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return Ok($"Read article with ID: {id}");
        }

        // POST: api/Article
        /*[HttpPost]
        public IActionResult Create()
        {
            return Ok("Article created");
        }*/

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

        database.InsertArticle(article);

        return CreatedAtAction(nameof(Get), new { id = article.Id, continent = article.Continent }, article);
    }

        // PUT: api/Article/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id)
        {
            return Ok($"Article with ID: {id} updated");
        }

        // DELETE: api/Article/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return Ok($"Article with ID: {id} deleted");
        }
    }
}