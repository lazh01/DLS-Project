namespace CommentService.Repositories
{
    public interface ICommentRepository
    {
        Task<List<Comment>> GetCommentsByArticleAndContinent(long articleId, string continent);
        Task<Comment> AddComment(CreateCommentRequest comment);
        Task<int> DeleteComment(long commentId, long articleId, string continent);
        Task<int> DeleteArticle(long articleId, string continent);
    }
}
