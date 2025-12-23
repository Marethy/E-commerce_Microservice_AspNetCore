using Payment.API.Entities;
using Payment.API.Services.Interfaces;
using Shared.DTOs.Payment;
using System.Security.Cryptography;
using System.Text;

namespace Payment.API.Services.Gateways
{
    /// <summary>
    /// VNPay Payment Gateway Implementation
    /// 
    /// 🔧 INTEGRATION GUIDE:
    /// 1. Install NuGet package: Install-Package VnPayLibrary (or use official SDK)
    /// 2. Get credentials from VNPay merchant portal:
    ///    - TmnCode (Terminal/Merchant Code)
    ///    - HashSecret (Secret key for HMAC SHA512)
    ///    - ReturnUrl (Callback URL after payment)
    /// 3. Add to appsettings.json:
    ///    "VNPaySettings": {
    ///      "TmnCode": "YOUR_TMN_CODE",
    ///      "HashSecret": "YOUR_HASH_SECRET",
    ///      "Url": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",  // Sandbox
    ///      // "Url": "https://vnpayment.vn/paymentv2/vpcpay.html",      // Production
    ///      "ReturnUrl": "https://yoursite.com/api/payments/vnpay-callback",
    ///      "Version": "2.1.0"
    ///    }
    /// 4. Register in ServiceExtensions.cs:
    ///    services.AddScoped<IPaymentGateway, VNPayGateway>();
    /// 
    /// 📚 Official docs: https://sandbox.vnpayment.vn/apis/docs/huong-dan-tich-hop/
    /// </summary>
    public class VNPayGateway : IPaymentGateway
    {
        private readonly ILogger<VNPayGateway> _logger;
        private readonly IConfiguration _configuration;

        // 🔧 TODO: Inject VNPaySettings from configuration
        // private readonly VNPaySettings _vnpaySettings;

        public string GatewayName => "VNPay";

        public VNPayGateway(ILogger<VNPayGateway> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            // _vnpaySettings = configuration.GetSection("VNPaySettings").Get<VNPaySettings>();
        }

        public async Task<PaymentGatewayResult> ProcessPaymentAsync(PaymentEntity payment, Dictionary<string, string>? gatewayData)
        {
            // VNPay is redirect-based, so we create payment URL instead
            var paymentUrl = await CreatePaymentUrlAsync(payment, gatewayData);
            
            return new PaymentGatewayResult
            {
                Success = true,
                TransactionId = payment.PaymentId, // Temporary, real TxnRef comes from callback
                PaymentUrl = paymentUrl,
                AdditionalData = new Dictionary<string, string>
                {
                    { "redirectRequired", "true" }
                }
            };
        }

        public async Task<string?> CreatePaymentUrlAsync(PaymentEntity payment, Dictionary<string, string>? gatewayData)
        {
            try
            {
                _logger.LogInformation("Creating VNPay payment URL for payment: {PaymentId}", payment.PaymentId);

                // 🔧 STEP 1: Get VNPay configuration
                var tmnCode = "YOUR_TMN_CODE"; // _vnpaySettings.TmnCode;
                var hashSecret = "YOUR_HASH_SECRET"; // _vnpaySettings.HashSecret;
                var vnpayUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"; // _vnpaySettings.Url;
                var returnUrl = "https://yoursite.com/api/payments/vnpay-callback"; // _vnpaySettings.ReturnUrl;

                // 🔧 STEP 2: Build request parameters
                var vnp = new SortedDictionary<string, string>
                {
                    { "vnp_Version", "2.1.0" },
                    { "vnp_Command", "pay" },
                    { "vnp_TmnCode", tmnCode },
                    { "vnp_Amount", ((long)(payment.Amount * 100)).ToString() }, // VNPay uses smallest unit (VND = amount * 100)
                    { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                    { "vnp_CurrCode", "VND" }, // VNPay only supports VND
                    { "vnp_IpAddr", gatewayData?.GetValueOrDefault("ipAddress") ?? "127.0.0.1" },
                    { "vnp_Locale", "vn" }, // vn | en
                    { "vnp_OrderInfo", $"Thanh toan don hang #{payment.OrderId}" },
                    { "vnp_OrderType", "other" }, // billpayment | fashion | other
                    { "vnp_ReturnUrl", returnUrl },
                    { "vnp_TxnRef", payment.PaymentId }, // Unique transaction reference
                };

                // 🔧 STEP 3: Create query string
                var queryString = string.Join("&", vnp.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

                // 🔧 STEP 4: Generate secure hash (HMAC SHA512)
                var secureHash = ComputeHmacSha512(hashSecret, queryString);
                
                // 🔧 STEP 5: Build final payment URL
                var paymentUrl = $"{vnpayUrl}?{queryString}&vnp_SecureHash={secureHash}";

                _logger.LogInformation("VNPay payment URL created successfully");
                
                return await Task.FromResult(paymentUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating VNPay payment URL");
                throw;
            }
        }

        public async Task<PaymentGatewayResult> VerifyPaymentCallbackAsync(Dictionary<string, string> callbackData)
        {
            try
            {
                // 🔧 STEP 1: Extract callback parameters
                var vnp_SecureHash = callbackData.GetValueOrDefault("vnp_SecureHash") ?? "";
                var vnp_ResponseCode = callbackData.GetValueOrDefault("vnp_ResponseCode") ?? "";
                var vnp_TxnRef = callbackData.GetValueOrDefault("vnp_TxnRef") ?? "";
                var vnp_TransactionNo = callbackData.GetValueOrDefault("vnp_TransactionNo") ?? "";

                // 🔧 STEP 2: Remove hash from data to verify
                var dataToVerify = callbackData.Where(kvp => kvp.Key != "vnp_SecureHash")
                                               .OrderBy(kvp => kvp.Key)
                                               .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                // 🔧 STEP 3: Compute expected hash
                var hashSecret = "YOUR_HASH_SECRET"; // _vnpaySettings.HashSecret;
                var queryString = string.Join("&", dataToVerify.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                var expectedHash = ComputeHmacSha512(hashSecret, queryString);

                // 🔧 STEP 4: Verify hash
                if (vnp_SecureHash != expectedHash)
                {
                    return new PaymentGatewayResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid signature"
                    };
                }

                // 🔧 STEP 5: Check response code
                // 00: Success, other codes: See VNPay documentation
                var success = vnp_ResponseCode == "00";
                
                return new PaymentGatewayResult
                {
                    Success = success,
                    TransactionId = vnp_TransactionNo,
                    ErrorMessage = success ? null : $"VNPay error code: {vnp_ResponseCode}",
                    AdditionalData = callbackData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying VNPay callback");
                return new PaymentGatewayResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<PaymentGatewayResult> ProcessRefundAsync(PaymentEntity payment, decimal refundAmount, string reason)
        {
            // 🔧 TODO: Implement VNPay refund API
            // Reference: https://sandbox.vnpayment.vn/apis/docs/hoan-tien/
            
            _logger.LogWarning("VNPay refund not implemented yet");
            
            return await Task.FromResult(new PaymentGatewayResult
            {
                Success = false,
                ErrorMessage = "VNPay refund not implemented"
            });
        }

        /// <summary>
        /// Compute HMAC SHA512 hash
        /// </summary>
        private string ComputeHmacSha512(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            
            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(dataBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    /// <summary>
    /// VNPay configuration settings
    /// </summary>
    public class VNPaySettings
    {
        public string TmnCode { get; set; } = string.Empty;
        public string HashSecret { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string Version { get; set; } = "2.1.0";
    }
}
