using Polly;
using Polly.CircuitBreaker;
using RestSharp;
using System.Text.Json;

namespace CommentService.Repositories
{

    public class ProfanityDetectedException : Exception
    {
        public ProfanityDetectedException(string message) : base(message) { }
    }

    public class CommentDbRepository : ICommentRepository
    {
        private readonly Database _db;
        private static RestClient restClient = new RestClient("http://profanityservice");
        private static readonly AsyncPolicy policy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(4, TimeSpan.FromSeconds(30),
            (exception, duration) =>
            {
                Console.WriteLine("Circuit breaker tripped");
            },
            () => Console.WriteLine("Circuit breaker reset")
            );

        

        public CommentDbRepository(Database db)
        {
            _db = db;
        }

        public async Task RecreateDatabase()
        {
            await _db.RecreateDatabase();
        }
        public async Task DeleteDatabase()
        {
            await _db.DeleteDatabase();
        }

        public async Task<Comment> AddComment(CreateCommentRequest com)
        {
            var response = policy.ExecuteAsync(async () =>
            {
                var request = new RestRequest("profanity/check-text", Method.Post);
                request.AddJsonBody(new { text = com.TextContent });
                var resp = await restClient.ExecuteAsync(request);

                if (!resp.IsSuccessful)
                {
                    throw new Exception($"Failed to check profanity: {resp.StatusCode} - {resp.Content}");
                }

                return resp; // only clean responses return here
            }).GetAwaiter().GetResult();
            Console.WriteLine($"Response from profanity service: {response.Content}");
            var jsonDoc = JsonDocument.Parse(response.Content!);
            bool containsProfanity = jsonDoc.RootElement.GetProperty("containsProfanity").GetBoolean();

            if (containsProfanity)
            {
                throw new ProfanityDetectedException("Comment contains profanity and cannot be created.");
            }
            var comment = await _db.InsertCommentAsync(com);
            return comment;
        }

        public async Task<List<Comment>> GetCommentsByArticleAndContinent(long articleId, string continent)
        {
            return await Database.FetchCommentsAsync(articleId, continent);
        }

        public async Task<int> DeleteComment(long commentId, long articleId, string continent)
        {
            return await _db.DeleteCommentAsync(commentId, articleId, continent);
        }

        public async Task<int> DeleteArticle(long articleId, string continent)
        {
            return await _db.DeleteArticleAsync(articleId, continent);
        }
    }
}
