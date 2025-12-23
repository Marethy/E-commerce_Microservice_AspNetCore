# 💳 Payment Service Integration Guide

## 🎯 Overview

Payment service đã được tích hợp đầy đủ vào cả backend và frontend với support cho:
- ✅ Mock Payment (testing)
- ✅ VNPay (Vietnamese payment gateway)
- ✅ MoMo (E-wallet)
- ✅ Các gateway khác (Stripe, PayPal - dễ dàng mở rộng)

---

## 🔧 Backend Integration

### 1. VNPay Integration

#### Bước 1: Cấu hình trong `appsettings.json`

```json
{
  "VNPaySettings": {
    "TmnCode": "YOUR_TMN_CODE",
    "HashSecret": "YOUR_HASH_SECRET",
    "Url": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "ReturnUrl": "https://yoursite.com/api/payments/vnpay-callback",
    "Version": "2.1.0"
  }
}
```

#### Bước 2: Uncomment code trong `PaymentService.cs`

Tìm method `ProcessPaymentGatewayAsync` và uncomment:

```csharp
switch (payment.PaymentGateway?.ToUpper())
{
    case "VNPAY":
        var vnpayGateway = new VNPayGateway(_logger, _configuration);
        return await vnpayGateway.ProcessPaymentAsync(payment, gatewayData);
    // ...
}
```

#### Bước 3: Tạo Callback Controller

```csharp
[HttpGet("vnpay-callback")]
public async Task<IActionResult> VNPayCallback([FromQuery] Dictionary<string, string> queryParams)
{
    var gateway = new VNPayGateway(_logger, _configuration);
    var result = await gateway.VerifyPaymentCallbackAsync(queryParams);
    
    // Update payment status in database
    if (result.Success)
    {
        // Publish PaymentSuccessEvent
    }
    
    return Redirect($"/order-success?paymentId={result.TransactionId}");
}
```

### 2. MoMo Integration

#### Bước 1: Cấu hình trong `appsettings.json`

```json
{
  "MoMoSettings": {
    "PartnerCode": "YOUR_PARTNER_CODE",
    "AccessKey": "YOUR_ACCESS_KEY",
    "SecretKey": "YOUR_SECRET_KEY",
    "Endpoint": "https://test-payment.momo.vn/v2/gateway/api/create",
    "ReturnUrl": "https://yoursite.com/api/payments/momo-callback",
    "IpnUrl": "https://yoursite.com/api/payments/momo-ipn",
    "RequestType": "captureWallet"
  }
}
```

#### Bước 2: Register IHttpClientFactory

Trong `ServiceExtensions.cs`:

```csharp
services.AddHttpClient();
```

#### Bước 3: Sử dụng MoMoGateway

```csharp
case "MOMO":
    var momoGateway = new MoMoGateway(_logger, _configuration, _httpClientFactory);
    return await momoGateway.ProcessPaymentAsync(payment, gat ewayData);
```

---

## 🎨 Frontend Integration

### 1. Process Payment with Redirect

```typescript
import { useProcessPayment } from '~/hooks/queries/usePaymentQueries';
import { PaymentMethod } from '~/api/services/paymentService';

function CheckoutPage() {
  const processPayment = useProcessPayment();

  const handlePayment = async () => {
    const result = await processPayment.mutateAsync({
      orderId: 123,
      username: user.username,
      emailAddress: user.email,
      amount: 100.00,
      paymentMethod: PaymentMethod.VNPay // or PaymentMethod.MoMo
    });

    // 🔧 IMPORTANT: For VNPay/MoMo, redirect to payment URL
    if (result.paymentUrl) {
      window.location.href = result.paymentUrl;
    }
  };

  return <button onClick={handlePayment}>Thanh toán VNPay</button>;
}
```

### 2. Complete Checkout Flow

```typescript
import { useCheckoutWithPayment } from '~/hooks/queries/usePaymentQueries';

function CompleteCheckout() {
  const checkoutWithPayment = useCheckoutWithPayment();

  const handleCheckout = async () => {
    await checkoutWithPayment({
      orderId: 123,
      username: user.username,
      emailAddress: user.email,
      totalAmount: 100.00,
      paymentMethod: PaymentMethod.VNPay
    });
    // Will auto-redirect to VNPay if needed
  };

  return <button onClick={handleCheckout}>Hoàn tất đơn hàng</button>;
}
```

### 3. Check Payment Status

```typescript
import { useOrderPayment } from '~/hooks/queries/usePaymentQueries';

function OrderDetailsPage({ orderId }: { orderId: number }) {
  const { data: payment, isLoading } = useOrderPayment(orderId);

  if (isLoading) return <div>Loading...</div>;

  return (
    <div>
      <p>Trạng thái: {payment?.status}</p>
      <p>Mã giao dịch: {payment?.transactionId}</p>
    </div>
  );
}
```

### 4. View Payment History

```typescript
import { usePaymentHistory } from '~/hooks/queries/usePaymentQueries';

function PaymentHistoryPage() {
  const { data: payments } = usePaymentHistory(user.username);

  return (
    <div>
      {payments?.map(payment => (
        <div key={payment.paymentId}>
          <p>Order #{payment.orderId}: {payment.amount} {payment.currency}</p>
          <p>Status: {payment.status}</p>
        </div>
      ))}
    </div>
  );
}
```

---

## 🔄 Payment Flow Diagram

```
1. User clicks "Thanh toán"
   ↓
2. Frontend calls processPayment()
   ↓
3. Backend creates payment record (Processing)
   ↓
4. [VNPay/MoMo] Backend creates payment URL
   ↓
5. Backend returns PaymentResponse with paymentUrl
   ↓
6. Frontend redirects to paymentUrl
   ↓
7. User completes payment on VNPay/MoMo
   ↓
8. VNPay/MoMo redirects back to callback URL
   ↓
9. Backend verifies callback signature
   ↓
10. Backend updates payment status to Success
    ↓
11. Backend publishes PaymentSuccessEvent
    ↓
12. Hangfire sends confirmation email
    ↓
13. User sees success page
```

---

## 📧 Email Notification Setup

### Create Hangfire Consumer

```csharp
public class PaymentSuccessConsumer : IConsumer<PaymentSuccessEvent>
{
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _templateService;

    public async Task Consume(ConsumeContext<PaymentSuccessEvent> context)
    {
        var payment = context.Message;
        
        // Read email template
        var template = _templateService.ReadEmailTemplateContent("payment-success");
        
        // Replace placeholders
        template = template
            .Replace("[userName]", payment.Username)
            .Replace("[orderId]", payment.OrderId.ToString())
            .Replace("[amount]", payment.Amount.ToString())
            .Replace("[currency]", payment.Currency)
            .Replace("[paymentMethod]", payment.PaymentMethod.ToString())
            .Replace("[transactionId]", payment.TransactionId)
            .Replace("[paymentTime]", payment.CompletedAt.ToString());
        
        // Send email
        await _emailService.SendEmailAsync(
            payment.EmailAddress,
            "Thanh toán thành công",
            template
        );
    }
}
```

### Register Consumer in Hangfire

```csharp
services.AddMassTransit(config =>
{
    config.AddConsumer<PaymentSuccessConsumer>();
    
    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(eventBusSettings!.HostAddress);
        cfg.ConfigureEndpoints(ctx);
    });
});
```

---

## 🧪 Testing

### 1. Test Mock Payment

```bash
POST http://localhost:5000/api/payments/process
Content-Type: application/json

{
  "orderId": 1,
  "username": "test@example.com",
  "emailAddress": "test@example.com",
  "amount": 100.00,
  "currency": "USD",
  "paymentMethod": 1,
  "paymentGateway": "Mock"
}
```

### 2. Test VNPay Payment

```bash
POST http://localhost:5000/api/payments/process
Content-Type: application/json

{
  "orderId": 1,
  "username": "test@example.com",
  "emailAddress": "test@example.com",
  "amount": 100.00,
  "currency": "VND",
  "paymentMethod": 7,
  "paymentGateway": "VNPay"
}
```

Expected response:
```json
{
  "paymentId": "abc123",
  "orderId": 1,
  "status": 2,
  "message": "Payment processed successfully",
  "paymentUrl": "https://sandbox.vnpayment.vn/...",
  "amount": 100.00,
  "currency": "VND"
}
```

---

## 🔐 Security Notes

1. **Always verify signatures** in callbacks (HMAC SHA256/SHA512)
2. **Use HTTPS** in production
3. **Store secrets** in environment variables or Azure Key Vault
4. **Implement IP whitelist** for webhook endpoints
5. **Add idempotency** to prevent duplicate payments

---

## 📚 Additional Resources

- **VNPay Docs**: https://sandbox.vnpayment.vn/apis/docs/huong-dan-tich-hop/
- **MoMo Docs**: https://developers.momo.vn/
- **Payment Service Code**: `src/Services/Payment/Payment.API/`
- **Frontend Integration**: `app/api/services/paymentService.ts`
