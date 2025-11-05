# Simple Test: Just verify Hangfire email scheduling works
Write-Host "?? Simple Hangfire Test - Email Scheduling" -ForegroundColor Cyan

# Scenario: User adds item to cart ? schedule reminder 10 seconds later
Write-Host "`n?? Simulating abandoned cart reminder..." -ForegroundColor Yellow

$emailHtml = @"
<!DOCTYPE html>
<html>
<head>
<style>
    body { font-family: Arial, sans-serif; }
    .container { max-width: 600px; margin: 0 auto; padding: 20px; }
    .header { background: #FF9800; color: white; padding: 20px; text-align: center; }
    .button { background: #4CAF50; color: white; padding: 12px 30px; text-decoration: none; 
          border-radius: 5px; display: inline-block; margin: 20px 0; }
</style>
</head>
<body>
<div class="container">
        <div class="header">
     <h1>?? You left items in your cart!</h1>
        </div>
        <div style="padding: 20px;">
      <p>Hi there,</p>
            <p>We noticed you left some items in your cart. Complete your purchase now!</p>
       
            <div style="background: #f9f9f9; padding: 15px; margin: 20px 0;">
                <h3>Your Cart:</h3>
  <ul>
            <li>Test Product x2 - $199.98</li>
 </ul>
       <p><strong>Total: $199.98</strong></p>
      </div>
       
      <a href="http://localhost/checkout" class="button">Complete Your Purchase</a>
  
            <p style="color: #777; font-size: 12px; margin-top: 30px;">
 This is an automated email. Your cart will be saved for 48 hours.
      </p>
        </div>
  </div>
</body>
</html>
"@

$scheduleTime = (Get-Date).AddSeconds(10).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")

$body = @{
    email = "testuser@example.com"
    subject = "?? You forgot something in your cart!"
    content = $emailHtml
    enqueueAt = $scheduleTime
} | ConvertTo-Json

Write-Host "?? Scheduling email for: $scheduleTime" -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri "http://localhost:6008/api/ScheduledJobs/send-reminder-email" `
      -Method Post `
        -ContentType "application/json" `
        -Body $body
    
Write-Host "? Email scheduled!" -ForegroundColor Green
    Write-Host "?? JobId: $response" -ForegroundColor Green
    
    Write-Host "`n? Waiting 15 seconds for email to be sent..." -ForegroundColor Yellow
    
    for ($i = 15; $i -gt 0; $i--) {
    Write-Host "   $i seconds remaining..." -ForegroundColor Gray
        Start-Sleep -Seconds 1
    }
    
  Write-Host "`n? Email should have been sent!" -ForegroundColor Green
    Write-Host "?? Check Hangfire Dashboard: http://localhost:6008/jobs" -ForegroundColor Cyan
    Write-Host "?? Check SMTP logs or configure real email to see result" -ForegroundColor Cyan
    
} catch {
    Write-Host "? Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n?? Test completed!" -ForegroundColor Green
