using Polly;
using Polly.CircuitBreaker;
using RestSharp;
using System.Text.Json;
namespace CommentService
{
    public class CommentManager
    {
        private static RestClient restClient = new RestClient("http://profanityservice");

        private static readonly AsyncPolicy policy1 = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        private static readonly AsyncPolicy policy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(4, TimeSpan.FromSeconds(30),
            (exception, duration) =>
            {
                Console.WriteLine("Circuit breaker tripped");
            },
            () => Console.WriteLine("Circuit breaker reset")
            );

        public async Task<bool> CreateComment(CreateCommentRequest com)
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
                return false;
            } else
            {
                try
                {
                    var db = Database.GetInstance();
                    var task = await db.InsertCommentAsync(com);
                } catch (Exception ex)
                {
                    Console.WriteLine($"Database connection error: {ex.Message}");
                    throw;
                }
                return true;
            }

        }
    }
}
