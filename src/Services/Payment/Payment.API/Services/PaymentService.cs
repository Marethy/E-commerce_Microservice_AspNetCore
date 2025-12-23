using EventBus.Messages.Events;
using MassTransit;
using Payment.API.Entities;
using Payment.API.Repositories.Interfaces;
using Payment.API.Services.Interfaces;
using Shared.DTOs.Payment;

namespace Payment.API.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentRepository repository,
            IPublishEndpoint publishEndpoint,
            ILogger<PaymentService> logger)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Processing payment for order: {OrderId}, Amount: {Amount}", request.OrderId, request.Amount);

                // Create payment entity
                var payment = new PaymentEntity
                {
                    PaymentId = Guid.NewGuid().ToString(),
                    OrderId = request.OrderId,
                    Username = request.Username,
                    EmailAddress = request.EmailAddress,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    PaymentMethod = request.PaymentMethod,
                    Status = PaymentStatus.Processing,
                    PaymentGateway = request.PaymentGateway ?? "Mock",
                    CreatedAt = DateTime.UtcNow
                };

                // Save to repository
                await _repository.CreateAsync(payment);

                // Process payment based on gateway
                var paymentResult = await ProcessPaymentGatewayAsync(payment, request.GatewayData);

                // Update payment status
                payment.Status = paymentResult.Success ? PaymentStatus.Success : PaymentStatus.Failed;
                payment.TransactionId = paymentResult.TransactionId;
                payment.PaymentUrl = paymentResult.PaymentUrl;
                payment.CompletedAt = paymentResult.Success ? DateTime.UtcNow : null;
                
                await _repository.UpdateAsync(payment);

                // Publish events
                if (paymentResult.Success)
                {
                    await PublishPaymentSuccessEventAsync(payment);
                }
                else
                {
                    await PublishPaymentFailedEventAsync(payment, paymentResult.ErrorMessage ?? "Payment processing failed");
                }

                // Return response
                return new PaymentResponse
                {
                    PaymentId = payment.PaymentId,
                    OrderId = payment.OrderId,
                    Status = payment.Status,
                    Message = paymentResult.Success ? "Payment processed successfully" : paymentResult.ErrorMessage ?? "Payment failed",
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    CreatedAt = payment.CreatedAt,
                    CompletedAt = payment.CompletedAt,
                    PaymentUrl = payment.PaymentUrl,
                    TransactionId = payment.TransactionId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for order: {OrderId}", request.OrderId);
                throw;
            }
        }

        public async Task<PaymentStatusDto?> GetPaymentAsync(string paymentId)
        {
            var payment = await _repository.GetByIdAsync(paymentId);
            return payment == null ? null : MapToStatusDto(payment);
        }

        public async Task<PaymentStatusDto?> GetPaymentByOrderIdAsync(int orderId)
        {
            var payment = await _repository.GetByOrderIdAsync(orderId);
            return payment == null ? null : MapToStatusDto(payment);
        }

        public async Task<List<PaymentStatusDto>> GetPaymentHistoryAsync(string username)
        {
            var payments = await _repository.GetByUsernameAsync(username);
            return payments.Select(MapToStatusDto).ToList();
        }

        public async Task<PaymentResponse> RefundPaymentAsync(RefundRequest request)
        {
            try
            {
                var payment = await _repository.GetByIdAsync(request.PaymentId);
                if (payment == null)
                {
                    throw new InvalidOperationException($"Payment {request.PaymentId} not found");
                }

                if (payment.Status != PaymentStatus.Success)
                {
                    throw new InvalidOperationException($"Cannot refund payment with status: {payment.Status}");
                }

                var refundAmount = request.Amount ?? payment.Amount;

                // Mock refund - in production, call actual payment gateway
                _logger.LogInformation("Processing refund for payment: {PaymentId}, Amount: {Amount}", request.PaymentId, refundAmount);

                payment.Status = refundAmount >= payment.Amount ? PaymentStatus.Refunded : PaymentStatus.PartialRefund;
                payment.RefundedAmount = (payment.RefundedAmount ?? 0) + refundAmount;
                payment.RefundReason = request.Reason;
                payment.RefundedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(payment);

                // Publish refund event
                await PublishPaymentRefundedEventAsync(payment, refundAmount, request.Reason);

                return new PaymentResponse
                {
                    PaymentId = payment.PaymentId,
                    OrderId = payment.OrderId,
                    Status = payment.Status,
                    Message = $"Refund of {refundAmount} {payment.Currency} processed successfully",
                    Amount = refundAmount,
                    Currency = payment.Currency,
                    CreatedAt = payment.CreatedAt,
                    CompletedAt = payment.RefundedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for payment: {PaymentId}", request.PaymentId);
                throw;
            }
        }

        #region Private Methods

        /// <summary>
        /// Process payment through payment gateway
        /// 
        /// 🔧 INTEGRATION GUIDE - Replace this mock with real gateway:
        /// 
        /// FOR VNPAY:
        /// 1. Uncomment VNPayGateway registration in ServiceExtensions.cs
        /// 2. Inject IPaymentGateway and use:
        ///    var vnpayGateway = new VNPayGateway(_logger, _configuration);
        ///    return await vnpayGateway.ProcessPaymentAsync(payment, gatewayData);
        /// 
        /// FOR MOMO:
        /// 1. Add IHttpClientFactory to constructor
        /// 2. Inject IPaymentGateway and use:
        ///    var momoGateway = new MoMoGateway(_logger, _configuration, _httpClientFactory);
        ///    return await momoGateway.ProcessPaymentAsync(payment, gatewayData);
        /// 
        /// GENERAL PATTERN (Recommended):
        /// 1. Use Strategy Pattern with Dictionary<string, IPaymentGateway>
        /// 2. Select gateway based on payment.PaymentGateway property
        ///    Example:
        ///    var gateway = _paymentGateways[payment.PaymentGateway];
        ///    return await gateway.ProcessPaymentAsync(payment, gatewayData);
        /// 
        /// IMPORTANT NOTES:
        /// - For redirect-based gateways (VNPay, MoMo), return PaymentUrl in the response
        /// - Client should redirect user to PaymentUrl for payment completion
        /// - Payment status becomes "Success" only after webhook/callback confirmation
        /// - Always verify signature/hash in callbacks to prevent fraud
        /// 
        /// See VNPayGateway.cs and MoMoGateway.cs for complete implementations
        /// </summary>
        private async Task<PaymentGatewayResult> ProcessPaymentGatewayAsync(PaymentEntity payment, Dictionary<string, string>? gatewayData)
        {
            // ✅ CURRENT: Mock payment processing for development/testing
            // ⚠️ TODO: Replace with real gateway integration before production
            
            _logger.LogInformation("Processing payment via {Gateway} for {Amount} {Currency}", 
                payment.PaymentGateway, payment.Amount, payment.Currency);

            // Simulate payment processing delay
            await Task.Delay(500);

            // 🔧 OPTION 1: Use switch statement for different gateways
            // switch (payment.PaymentGateway?.ToUpper())
            // {
            //     case "VNPAY":
            //         var vnpayGateway = new VNPayGateway(_logger, _configuration);
            //         return await vnpayGateway.ProcessPaymentAsync(payment, gatewayData);
            //     
            //     case "MOMO":
            //         var momoGateway = new MoMoGateway(_logger, _configuration, _httpClientFactory);
            //         return await momoGateway.ProcessPaymentAsync(payment, gatewayData);
            //     
            //     case "STRIPE":
            //         // Stripe integration here
            //         break;
            //     
            //     default:
            //         // Mock or throw exception
            //         break;
            // }

            // 🔧 OPTION 2: Use Strategy Pattern (Recommended for production)
            // Inject Dictionary<string, IPaymentGateway> in constructor
            // var gateway = _paymentGateways.GetValueOrDefault(payment.PaymentGateway ?? "Mock");
            // if (gateway != null)
            // {
            //     return await gateway.ProcessPaymentAsync(payment, gatewayData);
            // }

            // Mock: 95% success rate for testing
            var random = new Random();
            var success = random.Next(100) < 95;

            return new PaymentGatewayResult
            {
                Success = success,
                TransactionId = $"TXN_{Guid.NewGuid().ToString("N")[..16].ToUpper()}",
                // For VNPay/MoMo, include payment redirect URL
                PaymentUrl = payment.PaymentMethod == PaymentMethod.VNPay || payment.PaymentMethod == PaymentMethod.MoMo
                    ? $"https://mock-gateway.com/payment/{payment.PaymentId}"
                    : null,
                ErrorMessage = success ? null : "Insufficient funds or payment declined"
            };
        }

        private async Task PublishPaymentSuccessEventAsync(PaymentEntity payment)
        {
            var paymentSuccessEvent = new PaymentSuccessEvent
            {
                PaymentId = payment.PaymentId,
                OrderId = payment.OrderId,
                Username = payment.Username,
                EmailAddress = payment.EmailAddress,
                Amount = payment.Amount,
                Currency = payment.Currency,
                PaymentMethod = payment.PaymentMethod,
                TransactionId = payment.TransactionId,
                CompletedAt = payment.CompletedAt ?? DateTime.UtcNow
            };

            await _publishEndpoint.Publish(paymentSuccessEvent);
            _logger.LogInformation("Published PaymentSuccessEvent for payment: {PaymentId}", payment.PaymentId);
        }

        private async Task PublishPaymentFailedEventAsync(PaymentEntity payment, string reason)
        {
            var paymentFailedEvent = new PaymentFailedEvent
            {
                PaymentId = payment.PaymentId,
                OrderId = payment.OrderId,
                Username = payment.Username,
                EmailAddress = payment.EmailAddress,
                Amount = payment.Amount,
                Reason = reason,
                FailedAt = DateTime.UtcNow
            };

            await _publishEndpoint.Publish(paymentFailedEvent);
            _logger.LogInformation("Published PaymentFailedEvent for payment: {PaymentId}", payment.PaymentId);
        }

        private async Task PublishPaymentRefundedEventAsync(PaymentEntity payment, decimal refundAmount, string reason)
        {
            var paymentRefundedEvent = new PaymentRefundedEvent
            {
                PaymentId = payment.PaymentId,
                OrderId = payment.OrderId,
                Username = payment.Username,
                EmailAddress = payment.EmailAddress,
                RefundAmount = refundAmount,
                Reason = reason,
                RefundedAt = DateTime.UtcNow
            };

            await _publishEndpoint.Publish(paymentRefundedEvent);
            _logger.LogInformation("Published PaymentRefundedEvent for payment: {PaymentId}", payment.PaymentId);
        }

        private static PaymentStatusDto MapToStatusDto(PaymentEntity payment)
        {
            return new PaymentStatusDto
            {
                PaymentId = payment.PaymentId,
                OrderId = payment.OrderId,
                Username = payment.Username,
                Status = payment.Status,
                Amount = payment.Amount,
                Currency = payment.Currency,
                PaymentMethod = payment.PaymentMethod,
                CreatedAt = payment.CreatedAt,
                CompletedAt = payment.CompletedAt,
                TransactionId = payment.TransactionId
            };
        }

        #endregion

        private class PaymentGatewayResult
        {
            public bool Success { get; set; }
            public string? TransactionId { get; set; }
            public string? PaymentUrl { get; set; }
            public string? ErrorMessage { get; set; }
        }
    }
}
