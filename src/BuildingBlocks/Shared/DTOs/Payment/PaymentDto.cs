namespace Shared.DTOs.Payment
{
    public class ProcessPaymentRequest
    {
        public int OrderId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public PaymentMethod PaymentMethod { get; set; }
        
        // Optional: For payment gateway integration
        public string? PaymentGateway { get; set; } // "Mock", "VNPay", "MoMo", "Stripe"
        public Dictionary<string, string>? GatewayData { get; set; } // Extra data for gateway
    }

    public class PaymentResponse
    {
        public string PaymentId { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public PaymentStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        // For payment gateway redirect
        public string? PaymentUrl { get; set; }
        public string? TransactionId { get; set; }
    }

    public class RefundRequest
    {
        public string PaymentId { get; set; } = string.Empty;
        public decimal? Amount { get; set; } // Null = full refund
        public string Reason { get; set; } = string.Empty;
    }

    public class PaymentStatusDto
    {
        public string PaymentId { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public string Username { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? TransactionId { get; set; }
    }

    public enum PaymentMethod
    {
        CreditCard = 1,
        DebitCard = 2,
        PayPal = 3,
        BankTransfer = 4,
        CashOnDelivery = 5,
        MoMo = 6,
        VNPay = 7,
        Stripe = 8
    }

    public enum PaymentStatus
    {
        Pending = 1,
        Processing = 2,
        Success = 3,
        Failed = 4,
        Cancelled = 5,
        Refunded = 6,
        PartialRefund = 7
    }
}
