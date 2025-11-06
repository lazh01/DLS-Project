using System;
using System.ComponentModel.DataAnnotations;

namespace SubscriberService.Domain
{
    public class Subscriber
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = null!;

        public bool IsSubscribed { get; set; } = true;

        [Required]
        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UnsubscribedAt { get; set; }
    }
}