using Shared.DTOs.Payment;
using EventBus.Messages;

namespace EventBus.Messages.Events
{
    /// <summary>
    /// Event published when payment is successfully processed
    /// </summary>
    public record PaymentSuccessEvent : IntegrationBaseEvent
    {
        public string PaymentId { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public PaymentMethod PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public DateTime CompletedAt { get; set; }
    }

    /// <summary>
    /// Event published when payment fails
    /// </summary>
    public record PaymentFailedEvent : IntegrationBaseEvent
    {
        public string PaymentId { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime FailedAt { get; set; }
    }

    /// <summary>
    /// Event published when refund is processed
    /// </summary>
    public record PaymentRefundedEvent : IntegrationBaseEvent
    {
        public string PaymentId { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime RefundedAt { get; set; }
    }
}
