using Shared.DTOs.Payment;

namespace Payment.API.Entities
{
    public class PaymentEntity
    {
        public string PaymentId { get; set; } = Guid.NewGuid().ToString();
        public int OrderId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        
        // Payment Gateway info
        public string? PaymentGateway { get; set; }
        public string? TransactionId { get; set; }
        public string? PaymentUrl { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Refund info
        public decimal? RefundedAmount { get; set; }
        public string? RefundReason { get; set; }
        public DateTime? RefundedAt { get; set; }
        
        // Additional data
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
