// src/Services/Ordering/Ordering.API/EventBusConsumer/OrderStatusEmailConsumer.cs
using Contracts.ScheduledJobs;
using MassTransit;
using Shared.DTOs.ScheduledJob;
using ILogger = Serilog.ILogger;

namespace Ordering.API.EventBusConsumer;

public class OrderStatusEmailConsumer : IConsumer<OrderStatusChangedEvent>
{
    private readonly IScheduledJobsClient _hangfireClient;
    private readonly ILogger _logger;

    public OrderStatusEmailConsumer(IScheduledJobsClient hangfireClient, ILogger logger)
    {
        _hangfireClient = hangfireClient;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> context)
    {
        var orderEvent = context.Message;
        _logger.Information($"Processing order status change: Order {orderEvent.OrderId} ? {orderEvent.NewStatus}");

        try
        {
            switch (orderEvent.NewStatus)
            {
                case "Pending":
                    await SendOrderConfirmationEmail(orderEvent);
                    break;

                case "Confirmed":
                    await SendOrderConfirmationEmail(orderEvent);
                    break;

                case "Shipped":
                    await SendShipmentNotificationEmail(orderEvent);
                    await ScheduleDeliveryReminderEmail(orderEvent);
                    break;

                case "Delivered":
                    await SendDeliveryConfirmationEmail(orderEvent);
                    await ScheduleReviewReminderEmail(orderEvent);
                    break;

                case "Cancelled":
                    await SendCancellationEmail(orderEvent);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to send order status email for Order {orderEvent.OrderId}");
            throw; // Retry via MassTransit
        }
    }

    private async Task SendOrderConfirmationEmail(OrderStatusChangedEvent orderEvent)
    {
        var emailContent = GenerateOrderConfirmationHtml(orderEvent);
        
        var model = new ReminderEmailDto(
     orderEvent.CustomerEmail,
         $"Order Confirmation - #{orderEvent.OrderId}",
        emailContent,
            DateTimeOffset.UtcNow // Send immediately
        );

        var jobId = await _hangfireClient.SendReminderEmailAsync(model);
  _logger.Information($"Order confirmation email scheduled: JobId={jobId}");
    }

    private async Task SendShipmentNotificationEmail(OrderStatusChangedEvent orderEvent)
    {
        var emailContent = GenerateShipmentHtml(orderEvent);
        
   var model = new ReminderEmailDto(
         orderEvent.CustomerEmail,
            $"Your Order #{orderEvent.OrderId} Has Been Shipped! ??",
            emailContent,
            DateTimeOffset.UtcNow
        );

        await _hangfireClient.SendReminderEmailAsync(model);
    }

    private async Task ScheduleDeliveryReminderEmail(OrderStatusChangedEvent orderEvent)
{
        var estimatedDelivery = orderEvent.EstimatedDeliveryDate ?? DateTimeOffset.UtcNow.AddDays(3);
        var reminderTime = estimatedDelivery.AddHours(-12); // 12 hours before delivery

        var emailContent = GenerateDeliveryReminderHtml(orderEvent);
        
        var model = new ReminderEmailDto(
            orderEvent.CustomerEmail,
      $"Your Order #{orderEvent.OrderId} Arrives Tomorrow! ??",
            emailContent,
   reminderTime
        );

        var jobId = await _hangfireClient.SendReminderEmailAsync(model);
        _logger.Information($"Delivery reminder scheduled for {reminderTime}: JobId={jobId}");
  }

    private async Task ScheduleReviewReminderEmail(OrderStatusChangedEvent orderEvent)
    {
      var reminderTime = DateTimeOffset.UtcNow.AddDays(3); // 3 days after delivery

        var emailContent = GenerateReviewReminderHtml(orderEvent);
  
  var model = new ReminderEmailDto(
            orderEvent.CustomerEmail,
  $"How was your order? Share your feedback! ?",
    emailContent,
          reminderTime
      );

        var jobId = await _hangfireClient.SendReminderEmailAsync(model);
        _logger.Information($"Review reminder scheduled for {reminderTime}: JobId={jobId}");
    }

    private async Task SendDeliveryConfirmationEmail(OrderStatusChangedEvent orderEvent)
    {
        var emailContent = GenerateDeliveryConfirmationHtml(orderEvent);
        
        var model = new ReminderEmailDto(
        orderEvent.CustomerEmail,
         $"Order #{orderEvent.OrderId} Delivered Successfully! ?",
    emailContent,
            DateTimeOffset.UtcNow
        );

        await _hangfireClient.SendReminderEmailAsync(model);
    }

    private async Task SendCancellationEmail(OrderStatusChangedEvent orderEvent)
    {
     var emailContent = GenerateCancellationHtml(orderEvent);
     
        var model = new ReminderEmailDto(
 orderEvent.CustomerEmail,
       $"Order #{orderEvent.OrderId} Cancelled",
          emailContent,
            DateTimeOffset.UtcNow
     );

        await _hangfireClient.SendReminderEmailAsync(model);
  }

    #region HTML Template Generators

    private string GenerateOrderConfirmationHtml(OrderStatusChangedEvent order)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
<style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
  .header {{ background: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background: #f9f9f9; }}
        .order-details {{ background: white; padding: 15px; margin: 20px 0; border-radius: 5px; }}
        .footer {{ text-align: center; padding: 20px; color: #777; font-size: 12px; }}
        .button {{ background: #4CAF50; color: white; padding: 12px 30px; text-decoration: none; 
    border-radius: 5px; display: inline-block; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>? Order Confirmed!</h1>
        </div>
        <div class='content'>
    <p>Hi {order.CustomerName},</p>
            <p>Thank you for your order! We've received your order and it's being processed.</p>
     
         <div class='order-details'>
      <h3>Order Details</h3>
      <p><strong>Order Number:</strong> #{order.OrderId}</p>
   <p><strong>Order Date:</strong> {order.OrderDate:dd MMM yyyy}</p>
         <p><strong>Total Amount:</strong> ${order.TotalAmount:N2}</p>
    <p><strong>Estimated Delivery:</strong> {order.EstimatedDeliveryDate:dd MMM yyyy}</p>
 </div>
            
          <p>We'll notify you when your order ships.</p>
     
            <a href='https://yourstore.com/orders/{order.OrderId}' class='button'>Track Your Order</a>
        </div>
     <div class='footer'>
            <p>Questions? Contact us at support@yourstore.com</p>
    <p>&copy; 2024 Your E-Commerce Store</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateShipmentHtml(OrderStatusChangedEvent order)
  {
        return $@"
<!DOCTYPE html>
<html>
<body>
    <div class='container'>
        <div class='header' style='background: #2196F3;'>
            <h1>?? Your Order Has Shipped!</h1>
   </div>
        <div class='content'>
            <p>Hi {order.CustomerName},</p>
       <p>Great news! Your order #{order.OrderId} is on its way to you.</p>
     
    <div class='order-details'>
     <h3>Shipping Information</h3>
    <p><strong>Tracking Number:</strong> {order.TrackingNumber ?? "Available soon"}</p>
  <p><strong>Estimated Delivery:</strong> {order.EstimatedDeliveryDate:dd MMM yyyy}</p>
         <p><strong>Shipping Address:</strong><br/>{order.ShippingAddress}</p>
      </div>
          
 <a href='https://tracking.com/{order.TrackingNumber}' class='button' style='background: #2196F3;'>
   Track Shipment
            </a>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateDeliveryReminderHtml(OrderStatusChangedEvent order)
    {
   return $@"
<!DOCTYPE html>
<html>
<body>
    <div class='container'>
    <div class='header' style='background: #FF9800;'>
    <h1>?? Your Order Arrives Tomorrow!</h1>
      </div>
        <div class='content'>
            <p>Hi {order.CustomerName},</p>
     <p>Just a heads up - your order #{order.OrderId} will be delivered tomorrow!</p>
       <p>Make sure someone is available to receive the package.</p>
         
     <a href='https://yourstore.com/orders/{order.OrderId}' class='button' style='background: #FF9800;'>
    View Order Details
</a>
        </div>
    </div>
</body>
</html>";
    }

  private string GenerateReviewReminderHtml(OrderStatusChangedEvent order)
    {
   return $@"
<!DOCTYPE html>
<html>
<body>
    <div class='container'>
        <div class='header' style='background: #9C27B0;'>
         <h1>? How Was Your Order?</h1>
        </div>
        <div class='content'>
    <p>Hi {order.CustomerName},</p>
            <p>We hope you're enjoying your recent purchase (Order #{order.OrderId})!</p>
      <p>Your feedback helps us improve. Would you mind leaving a review?</p>
    
            <a href='https://yourstore.com/orders/{order.OrderId}/review' class='button' style='background: #9C27B0;'>
     Leave a Review
       </a>
   </div>
    </div>
</body>
</html>";
    }

    private string GenerateDeliveryConfirmationHtml(OrderStatusChangedEvent order)
    {
        return $@"
<!DOCTYPE html>
<html>
<body>
    <div class='container'>
        <div class='header' style='background: #4CAF50;'>
  <h1>? Order Delivered Successfully!</h1>
 </div>
        <div class='content'>
   <p>Hi {order.CustomerName},</p>
            <p>Your order #{order.OrderId} has been delivered!</p>
      <p>We hope you love your purchase. If you have any issues, please contact us.</p>
            
<a href='https://yourstore.com/orders/{order.OrderId}/review' class='button'>
Leave a Review
            </a>
  </div>
    </div>
</body>
</html>";
    }

  private string GenerateCancellationHtml(OrderStatusChangedEvent order)
    {
        return $@"
<!DOCTYPE html>
<html>
<body>
    <div class='container'>
        <div class='header' style='background: #f44336;'>
            <h1>Order Cancelled</h1>
        </div>
   <div class='content'>
            <p>Hi {order.CustomerName},</p>
            <p>Your order #{order.OrderId} has been cancelled as requested.</p>
  <p>If you didn't request this cancellation, please contact us immediately.</p>
    <p><strong>Refund:</strong> Your payment will be refunded within 5-7 business days.</p>
        </div>
  </div>
</body>
</html>";
    }

    #endregion
}

// Event DTO
public record OrderStatusChangedEvent(
    long OrderId,
    string CustomerName,
    string CustomerEmail,
    string OldStatus,
    string NewStatus,
    DateTime OrderDate,
    decimal TotalAmount,
    DateTimeOffset? EstimatedDeliveryDate,
    string? TrackingNumber,
    string? ShippingAddress
);
