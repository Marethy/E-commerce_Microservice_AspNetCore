using Microsoft.AspNetCore.Mvc;
using Payment.API.Services.Interfaces;
using Shared.DTOs.Payment;
using Shared.SeedWork.ApiResult;
using System.Net;

namespace Payment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// Process a payment for an order
        /// </summary>
        [HttpPost("process")]
        [ProducesResponseType(typeof(ApiResult<PaymentResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            try
            {
                _logger.LogInformation("API: Processing payment for order {OrderId}", request.OrderId);
                var result = await _paymentService.ProcessPaymentAsync(request);
                return Ok(new ApiSuccessResult<PaymentResponse>(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error processing payment for order {OrderId}", request.OrderId);
                return BadRequest(new ApiErrorResult<PaymentResponse>(ex.Message));
            }
        }

        /// <summary>
        /// Get payment details by payment ID
        /// </summary>
        [HttpGet("{paymentId}")]
        [ProducesResponseType(typeof(ApiResult<PaymentStatusDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetPayment(string paymentId)
        {
            try
            {
                var payment = await _paymentService.GetPaymentAsync(paymentId);
                if (payment == null)
                    return NotFound(new ApiErrorResult<PaymentStatusDto>($"Payment {paymentId} not found"));

                return Ok(new ApiSuccessResult<PaymentStatusDto>(payment));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting payment {PaymentId}", paymentId);
                return BadRequest(new ApiErrorResult<PaymentStatusDto>(ex.Message));
            }
        }

        /// <summary>
        /// Get payment by order ID
        /// </summary>
        [HttpGet("order/{orderId}")]
        [ProducesResponseType(typeof(ApiResult<PaymentStatusDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetPaymentByOrderId(int orderId)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
                if (payment == null)
                    return NotFound(new ApiErrorResult<PaymentStatusDto>($"Payment for order {orderId} not found"));

                return Ok(new ApiSuccessResult<PaymentStatusDto>(payment));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting payment for order {OrderId}", orderId);
                return BadRequest(new ApiErrorResult<PaymentStatusDto>(ex.Message));
            }
        }

        /// <summary>
        /// Get payment history for a user
        /// </summary>
        [HttpGet("history/{username}")]
        [ProducesResponseType(typeof(ApiResult<List<PaymentStatusDto>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetPaymentHistory(string username)
        {
            try
            {
                var payments = await _paymentService.GetPaymentHistoryAsync(username);
                return Ok(new ApiSuccessResult<List<PaymentStatusDto>>(payments));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting payment history for {Username}", username);
                return BadRequest(new ApiErrorResult<List<PaymentStatusDto>>(ex.Message));
            }
        }

        /// <summary>
        /// Process a refund for a payment
        /// </summary>
        [HttpPost("{paymentId}/refund")]
        [ProducesResponseType(typeof(ApiResult<PaymentResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RefundPayment(string paymentId, [FromBody] RefundRequest request)
        {
            try
            {
                request.PaymentId = paymentId; // Ensure consistency
                _logger.LogInformation("API: Processing refund for payment {PaymentId}", paymentId);
                var result = await _paymentService.RefundPaymentAsync(request);
                return Ok(new ApiSuccessResult<PaymentResponse>(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error processing refund for payment {PaymentId}", paymentId);
                return BadRequest(new ApiErrorResult<PaymentResponse>(ex.Message));
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", service = "Payment.API", timestamp = DateTime.UtcNow });
        }
    }
}
