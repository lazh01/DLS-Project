
    public class CreateCommentRequest
    {
        public long ArticleId { get; set; }

        public string ArticleContinent { get; set; } = null!;
        public string TextContent { get; set; } = null!;
        public string Username { get; set; } = null!;
    }

