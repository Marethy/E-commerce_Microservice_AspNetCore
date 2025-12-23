using Payment.API.Entities;
using Shared.DTOs.Payment;

namespace Payment.API.Services.Interfaces
{
    /// <summary>
    /// Payment Gateway Interface
    /// Implement this interface for each payment gateway (VNPay, MoMo, Stripe, etc.)
    /// </summary>
    public interface IPaymentGateway
    {
        string GatewayName { get; }
        
        /// <summary>
        /// Process payment through the gateway
        /// </summary>
        Task<PaymentGatewayResult> ProcessPaymentAsync(PaymentEntity payment, Dictionary<string, string>? gatewayData);
        
        /// <summary>
        /// Create payment URL for redirect-based gateways (VNPay, MoMo)
        /// </summary>
        Task<string?> CreatePaymentUrlAsync(PaymentEntity payment, Dictionary<string, string>? gatewayData);
        
        /// <summary>
        /// Verify payment callback/webhook from gateway
        /// </summary>
        Task<PaymentGatewayResult> VerifyPaymentCallbackAsync(Dictionary<string, string> callbackData);
        
        /// <summary>
        /// Process refund through the gateway
        /// </summary>
        Task<PaymentGatewayResult> ProcessRefundAsync(PaymentEntity payment, decimal refundAmount, string reason);
    }

    /// <summary>
    /// Result from payment gateway operations
    /// </summary>
    public class PaymentGatewayResult
    {
        public bool Success { get; set; }
        public string? TransactionId { get; set; }
        public string? PaymentUrl { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, string>? AdditionalData { get; set; }
    }
}
