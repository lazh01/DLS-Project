namespace NewsletterService.DTOs
{
    public class SubscriberDto
    {
        public string Id { get; set; } = null!;
        public string Username { get; set; } = null!;
        public bool IsSubscribed { get; set; }
        public DateTime SubscribedAt { get; set; }
        public DateTime? UnsubscribedAt { get; set; }
    }
}
