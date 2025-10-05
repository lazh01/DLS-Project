using RestSharp;
using Messages.SharedModels;

namespace NewsletterService.Services
{
    public class FetchArticlesService
    {
        private static RestClient restClient = new RestClient("http://articleservice");
        public async Task<List<ArticleDTO>> FetchArticlesAsync(FetchArticlesRequest request)
        {
            var rest_request = new RestRequest("api/Article/fetch", Method.Post);
            rest_request.AddJsonBody(request);
            var response = await restClient.ExecuteAsync(rest_request);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Failed to fetch articles: {response.StatusCode} - {response.Content}");
            }

            if (string.IsNullOrWhiteSpace(response.Content))
            {
                return new List<ArticleDTO>();
            }

            var articles = System.Text.Json.JsonSerializer.Deserialize<List<ArticleDTO>>(
                response.Content,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // ✅ handle camelCase JSON
                }
            );

            return articles ?? new List<ArticleDTO>();
        }
    }
}
