using Microsoft.Extensions.Caching.Distributed;
using Payment.API.Entities;
using Payment.API.Repositories.Interfaces;
using System.Text.Json;

namespace Payment.API.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<PaymentRepository> _logger;
        private const string PAYMENT_PREFIX = "payment:";
        private const string ORDER_PREFIX = "payment:order:";
        private const string USER_PREFIX = "payment:user:";

        public PaymentRepository(IDistributedCache cache, ILogger<PaymentRepository> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<PaymentEntity?> GetByIdAsync(string paymentId)
        {
            try
            {
                var key = $"{PAYMENT_PREFIX}{paymentId}";
                var json = await _cache.GetStringAsync(key);
                
                if (string.IsNullOrEmpty(json))
                    return null;

                return JsonSerializer.Deserialize<PaymentEntity>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment by ID: {PaymentId}", paymentId);
                throw;
            }
        }

        public async Task<PaymentEntity?> GetByOrderIdAsync(int orderId)
        {
            try
            {
                var key = $"{ORDER_PREFIX}{orderId}";
                var paymentId = await _cache.GetStringAsync(key);
                
                if (string.IsNullOrEmpty(paymentId))
                    return null;

                return await GetByIdAsync(paymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment by order ID: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<List<PaymentEntity>> GetByUsernameAsync(string username, int limit = 50)
        {
            try
            {
                var key = $"{USER_PREFIX}{username}";
                var json = await _cache.GetStringAsync(key);
                
                if (string.IsNullOrEmpty(json))
                    return new List<PaymentEntity>();

                var paymentIds = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                var payments = new List<PaymentEntity>();

                foreach (var paymentId in paymentIds.Take(limit))
                {
                    var payment = await GetByIdAsync(paymentId);
                    if (payment != null)
                        payments.Add(payment);
                }

                return payments.OrderByDescending(p => p.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments by username: {Username}", username);
                throw;
            }
        }

        public async Task<PaymentEntity> CreateAsync(PaymentEntity payment)
        {
            try
            {
                payment.CreatedAt = DateTime.UtcNow;
                var json = JsonSerializer.Serialize(payment);
                
                // Save payment
                var paymentKey = $"{PAYMENT_PREFIX}{payment.PaymentId}";
                await _cache.SetStringAsync(paymentKey, json, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(90) // Keep for 90 days
                });

                // Index by order ID
                var orderKey = $"{ORDER_PREFIX}{payment.OrderId}";
                await _cache.SetStringAsync(orderKey, payment.PaymentId, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(90)
                });

                // Index by username
                await AddToUserIndexAsync(payment.Username, payment.PaymentId);

                _logger.LogInformation("Created payment: {PaymentId} for order: {OrderId}", payment.PaymentId, payment.OrderId);
                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment: {PaymentId}", payment.PaymentId);
                throw;
            }
        }

        public async Task<PaymentEntity> UpdateAsync(PaymentEntity payment)
        {
            try
            {
                payment.UpdatedAt = DateTime.UtcNow;
                var json = JsonSerializer.Serialize(payment);
                
                var paymentKey = $"{PAYMENT_PREFIX}{payment.PaymentId}";
                await _cache.SetStringAsync(paymentKey, json, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(90)
                });

                _logger.LogInformation("Updated payment: {PaymentId}", payment.PaymentId);
                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment: {PaymentId}", payment.PaymentId);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string paymentId)
        {
            try
            {
                var payment = await GetByIdAsync(paymentId);
                if (payment == null)
                    return false;

                // Remove payment
                var paymentKey = $"{PAYMENT_PREFIX}{paymentId}";
                await _cache.RemoveAsync(paymentKey);

                // Remove order index
                var orderKey = $"{ORDER_PREFIX}{payment.OrderId}";
                await _cache.RemoveAsync(orderKey);

                // Remove from user index
                await RemoveFromUserIndexAsync(payment.Username, paymentId);

                _logger.LogInformation("Deleted payment: {PaymentId}", paymentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment: {PaymentId}", paymentId);
                throw;
            }
        }

        private async Task AddToUserIndexAsync(string username, string paymentId)
        {
            var key = $"{USER_PREFIX}{username}";
            var json = await _cache.GetStringAsync(key);
            var paymentIds = string.IsNullOrEmpty(json) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();

            if (!paymentIds.Contains(paymentId))
            {
                paymentIds.Add(paymentId);
                var updatedJson = JsonSerializer.Serialize(paymentIds);
                await _cache.SetStringAsync(key, updatedJson, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(90)
                });
            }
        }

        private async Task RemoveFromUserIndexAsync(string username, string paymentId)
        {
            var key = $"{USER_PREFIX}{username}";
            var json = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(json))
                return;

            var paymentIds = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            paymentIds.Remove(paymentId);
            
            var updatedJson = JsonSerializer.Serialize(paymentIds);
            await _cache.SetStringAsync(key, updatedJson, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(90)
            });
        }
    }
}
