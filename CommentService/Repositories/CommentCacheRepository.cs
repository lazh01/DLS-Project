using CommentService.Services;
using Microsoft.AspNetCore.DataProtection.Repositories;
using System.Reflection.Metadata.Ecma335;
namespace CommentService.Repositories
{
    public class CommentCacheRepository : ICommentRepository
    {
        private readonly CommentCacheService _cache;
        private readonly ICommentRepository _repository;

        public CommentCacheRepository(CommentCacheService cache, ICommentRepository repository)
        {
            _cache = cache;
            _repository = repository;
        }

        public async Task<Comment> AddComment(CreateCommentRequest com)
        {
            var comment = await _repository.AddComment(com);
            if (comment == null)
                return null;


            await _cache.AddCommentToCache(comment);
            return comment;
        }

        public async Task<int> DeleteArticle(long articleId, string continent)
        {
            var result = await _repository.DeleteArticle(articleId, continent);
            if (result > 0)
            {
                await _cache.RemoveArticleFromCache(articleId, continent);
            }
            return result;
        }

        public async Task<int> DeleteComment(long commentId, long articleId, string continent)
        {
            var result = await _repository.DeleteComment(commentId, articleId, continent);
            if (result > 0)
            {
                await _cache.RemoveCommentFromCache(articleId, continent, commentId);
            }
            return result;
        }

        public async Task<List<Comment>> GetCommentsByArticleAndContinent(long articleId, string continent)
        {
            var cachedComments = await _cache.GetCommentsByArticleAndContinent(articleId, continent);
            if (cachedComments != null)
                return cachedComments;
            var comments = await _repository.GetCommentsByArticleAndContinent(articleId, continent);
            foreach (var comment in comments)
            {
                await _cache.AddCommentToCache(comment);
            }
            return comments;
        }
    }
}
