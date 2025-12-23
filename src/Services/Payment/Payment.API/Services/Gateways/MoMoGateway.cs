using Payment.API.Entities;
using Payment.API.Services.Interfaces;
using Shared.DTOs.Payment;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Payment.API.Services.Gateways
{
    /// <summary>
    /// MoMo Payment Gateway Implementation
    /// 
    /// 🔧 INTEGRATION GUIDE:
    /// 1. Register merchant account at: https://business.momo.vn/
    /// 2. Get credentials from MoMo partner portal:
    ///    - PartnerCode
    ///    - AccessKey
    ///    - SecretKey
    /// 3. Add to appsettings.json:
    ///    "MoMoSettings": {
    ///      "PartnerCode": "YOUR_PARTNER_CODE",
    ///      "AccessKey": "YOUR_ACCESS_KEY",
    ///      "SecretKey": "YOUR_SECRET_KEY",
    ///      "Endpoint": "https://test-payment.momo.vn/v2/gateway/api/create",  // Sandbox
    ///      // "Endpoint": "https://payment.momo.vn/v2/gateway/api/create",     // Production
    ///      "ReturnUrl": "https://yoursite.com/api/payments/momo-callback",
    ///      "IpnUrl": "https://yoursite.com/api/payments/momo-ipn",
    ///      "RequestType": "captureWallet"
    ///    }
    /// 4. Register in ServiceExtensions.cs:
    ///    services.AddScoped<IPaymentGateway, MoMoGateway>();
    /// 
    /// 📚 Official docs: https://developers.momo.vn/
    /// </summary>
    public class MoMoGateway : IPaymentGateway
    {
        private readonly ILogger<MoMoGateway> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        // 🔧 TODO: Inject MoMoSettings from configuration
        // private readonly MoMoSettings _momoSettings;

        public string GatewayName => "MoMo";

        public MoMoGateway(
            ILogger<MoMoGateway> logger, 
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            // _momoSettings = configuration.GetSection("MoMoSettings").Get<MoMoSettings>();
        }

        public async Task<PaymentGatewayResult> ProcessPaymentAsync(PaymentEntity payment, Dictionary<string, string>? gatewayData)
        {
            // MoMo requires API call to create payment request
            try
            {
                // 🔧 STEP 1: Get MoMo configuration
                var partnerCode = "YOUR_PARTNER_CODE"; // _momoSettings.PartnerCode;
                var accessKey = "YOUR_ACCESS_KEY"; // _momoSettings.AccessKey;
                var secretKey = "YOUR_SECRET_KEY"; // _momoSettings.SecretKey;
                var endpoint = "https://test-payment.momo.vn/v2/gateway/api/create"; // _momoSettings.Endpoint;
                var returnUrl = "https://yoursite.com/api/payments/momo-callback"; // _momoSettings.ReturnUrl;
                var ipnUrl = "https://yoursite.com/api/payments/momo-ipn"; // _momoSettings.IpnUrl;

                // 🔧 STEP 2: Build request data
                var orderId = $"MM{DateTime.Now:yyyyMMddHHmmss}"; // MoMo order ID (unique)
                var requestId = payment.PaymentId; // Your transaction reference
                var amount = (long)payment.Amount; // MoMo uses exact amount (VND)
                var orderInfo = $"Thanh toán đơn hàng #{payment.OrderId}";
                var requestType = "captureWallet"; // captureWallet | payWithATM | payWithCC
                var extraData = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
                {
                    paymentId = payment.PaymentId,
                    orderId = payment.OrderId
                })));

                // 🔧 STEP 3: Create raw signature string
                var rawSignature = $"accessKey={accessKey}" +
                                $"&amount={amount}" +
                                $"&extraData={extraData}" +
                                $"&ipnUrl={ipnUrl}" +
                                $"&orderId={orderId}" +
                                $"&orderInfo={orderInfo}" +
                                $"&partnerCode={partnerCode}" +
                                $"&redirectUrl={returnUrl}" +
                                $"&requestId={requestId}" +
                                $"&requestType={requestType}";

                // 🔧 STEP 4: Generate signature (HMAC SHA256)
                var signature = ComputeHmacSha256(secretKey, rawSignature);

                // 🔧 STEP 5: Build request payload
                var requestData = new
                {
                    partnerCode,
                    accessKey,
                    requestId,
                    amount,
                    orderId,
                    orderInfo,
                    redirectUrl = returnUrl,
                    ipnUrl,
                    requestType,
                    extraData,
                    signature,
                    lang = "vi" // vi | en
                };

                // 🔧 STEP 6: Send POST request to MoMo API
                var httpClient = _httpClientFactory.CreateClient();
                var content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await httpClient.PostAsync(endpoint, content);
                var responseBody = await response.Content.ReadAsStringAsync();
                var momoResponse = JsonSerializer.Deserialize<MoMoCreatePaymentResponse>(responseBody);

                // 🔧 STEP 7: Check response
                if (momoResponse?.ResultCode == 0) // 0 = Success
                {
                    return new PaymentGatewayResult
                    {
                        Success = true,
                        TransactionId = orderId,
                        PaymentUrl = momoResponse.PayUrl,
                        AdditionalData = new Dictionary<string, string>
                        {
                            { "redirectRequired", "true" },
                            { "deeplink", momoResponse.Deeplink ?? "" }
                        }
                    };
                }

                return new PaymentGatewayResult
                {
                    Success = false,
                    ErrorMessage = momoResponse?.Message ?? "MoMo payment creation failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MoMo payment");
                return new PaymentGatewayResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<string?> CreatePaymentUrlAsync(PaymentEntity payment, Dictionary<string, string>? gatewayData)
        {
            // MoMo creates payment URL via API call, handled in ProcessPaymentAsync
            var result = await ProcessPaymentAsync(payment, gatewayData);
            return result.PaymentUrl;
        }

        public async Task<PaymentGatewayResult> VerifyPaymentCallbackAsync(Dictionary<string, string> callbackData)
        {
            try
            {
                // 🔧 STEP 1: Extract callback parameters
                var partnerCode = callbackData.GetValueOrDefault("partnerCode") ?? "";
                var orderId = callbackData.GetValueOrDefault("orderId") ?? "";
                var requestId = callbackData.GetValueOrDefault("requestId") ?? "";
                var amount = callbackData.GetValueOrDefault("amount") ?? "";
                var orderInfo = callbackData.GetValueOrDefault("orderInfo") ?? "";
                var orderType = callbackData.GetValueOrDefault("orderType") ?? "";
                var transId = callbackData.GetValueOrDefault("transId") ?? "";
                var resultCode = callbackData.GetValueOrDefault("resultCode") ?? "";
                var message = callbackData.GetValueOrDefault("message") ?? "";
                var payType = callbackData.GetValueOrDefault("payType") ?? "";
                var responseTime = callbackData.GetValueOrDefault("responseTime") ?? "";
                var extraData = callbackData.GetValueOrDefault("extraData") ?? "";
                var signature = callbackData.GetValueOrDefault("signature") ?? "";

                // 🔧 STEP 2: Build raw signature for verification
                var secretKey = "YOUR_SECRET_KEY"; // _momoSettings.SecretKey;
                var accessKey = "YOUR_ACCESS_KEY"; // _momoSettings.AccessKey;

                var rawSignature = $"accessKey={accessKey}" +
                                $"&amount={amount}" +
                                $"&extraData={extraData}" +
                                $"&message={message}" +
                                $"&orderId={orderId}" +
                                $"&orderInfo={orderInfo}" +
                                $"&orderType={orderType}" +
                                $"&partnerCode={partnerCode}" +
                                $"&payType={payType}" +
                                $"&requestId={requestId}" +
                                $"&responseTime={responseTime}" +
                                $"&resultCode={resultCode}" +
                                $"&transId={transId}";

                // 🔧 STEP 3: Compute expected signature
                var expectedSignature = ComputeHmacSha256(secretKey, rawSignature);

                // 🔧 STEP 4: Verify signature
                if (signature != expectedSignature)
                {
                    return new PaymentGatewayResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid signature"
                    };
                }

                // 🔧 STEP 5: Check result code
                // 0: Success, other codes: See MoMo documentation
                var success = resultCode == "0";

                return new PaymentGatewayResult
                {
                    Success = success,
                    TransactionId = transId,
                    ErrorMessage = success ? null : message,
                    AdditionalData = callbackData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying MoMo callback");
                return new PaymentGatewayResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<PaymentGatewayResult> ProcessRefundAsync(PaymentEntity payment, decimal refundAmount, string reason)
        {
            // 🔧 TODO: Implement MoMo refund API
            // Reference: https://developers.momo.vn/#/docs/refund
            
            _logger.LogWarning("MoMo refund not implemented yet");
            
            return await Task.FromResult(new PaymentGatewayResult
            {
                Success = false,
                ErrorMessage = "MoMo refund not implemented"
            });
        }

        /// <summary>
        /// Compute HMAC SHA256 hash
        /// </summary>
        private string ComputeHmacSha256(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            
            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(dataBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    /// <summary>
    /// MoMo configuration settings
    /// </summary>
    public class MoMoSettings
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string IpnUrl { get; set; } = string.Empty;
        public string RequestType { get; set; } = "captureWallet";
    }

    /// <summary>
    /// MoMo API response for create payment
    /// </summary>
    public class MoMoCreatePaymentResponse
    {
        public string? PartnerCode { get; set; }
        public string? RequestId { get; set; }
        public string? OrderId { get; set; }
        public long Amount { get; set; }
        public long ResponseTime { get; set; }
        public string? Message { get; set; }
        public int ResultCode { get; set; }
        public string? PayUrl { get; set; }
        public string? Deeplink { get; set; }
        public string? QrCodeUrl { get; set; }
    }
}
