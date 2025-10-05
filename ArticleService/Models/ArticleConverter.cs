using Messages.SharedModels;
namespace Articleservice.Models
{
    public class ArticleConverter
    {
        public static ArticleDTO ToDTO(Article article)
        {
            return new ArticleDTO
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                Author = article.Author,
                PublishedAt = article.PublishedAt,
                CreatedAt = article.CreatedAt,
                Continent = article.Continent
            };
        }
        public static Article FromDTO(ArticleDTO dto)
        {
            return new Article
            {
                Id = dto.Id,
                Title = dto.Title,
                Content = dto.Content,
                Author = dto.Author,
                PublishedAt = dto.PublishedAt,
                CreatedAt = dto.CreatedAt,
                Continent = dto.Continent
            };
        }
    }
}
