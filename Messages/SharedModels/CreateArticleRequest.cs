namespace SharedModels;
public class CreateArticleRequest
{
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Author { get; set; } = null!;
    public string Continent { get; set; } = null!;

    public string? TraceParent { get; set; }
    public string? TraceState { get; set; }
}