using Payment.API.Entities;

namespace Payment.API.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<PaymentEntity?> GetByIdAsync(string paymentId);
        Task<PaymentEntity?> GetByOrderIdAsync(int orderId);
        Task<List<PaymentEntity>> GetByUsernameAsync(string username, int limit = 50);
        Task<PaymentEntity> CreateAsync(PaymentEntity payment);
        Task<PaymentEntity> UpdateAsync(PaymentEntity payment);
        Task<bool> DeleteAsync(string paymentId);
    }
}
