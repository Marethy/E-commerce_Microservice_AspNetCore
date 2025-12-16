using Contracts.Domains;

namespace Customer.API.Entities
{
    public class Notification : AuditableEntity<Guid>
    {
        public string UserId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // order, delivery, promotion, system, review, wishlist, payment
        public string Priority { get; set; } = "medium"; // low, medium, high, urgent
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public string? ActionUrl { get; set; }
        public string? ActionLabel { get; set; }
        public string? ImageUrl { get; set; }
        public string? Metadata { get; set; } // JSON string for additional data
        public DateTimeOffset NotificationDate { get; set; } = DateTimeOffset.UtcNow;
    }
}
