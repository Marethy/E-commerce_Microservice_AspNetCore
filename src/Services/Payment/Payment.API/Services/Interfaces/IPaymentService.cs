using Payment.API.Entities;
using Shared.DTOs.Payment;

namespace Payment.API.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request);
        Task<PaymentStatusDto?> GetPaymentAsync(string paymentId);
        Task<PaymentStatusDto?> GetPaymentByOrderIdAsync(int orderId);
        Task<List<PaymentStatusDto>> GetPaymentHistoryAsync(string username);
        Task<PaymentResponse> RefundPaymentAsync(RefundRequest request);
    }
}
