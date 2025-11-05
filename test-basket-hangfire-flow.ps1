# Test Full Flow: Basket API ? Hangfire API
# This tests the real abandoned cart reminder flow

Write-Host "?? Testing Basket ? Hangfire Flow..." -ForegroundColor Cyan

# Test: Update basket (should trigger reminder email schedule)
Write-Host "`n1??  Test: Add item to basket (triggers email reminder)" -ForegroundColor Yellow

$basket = @{
    username = "testuser"
    emailAddress = "testuser@example.com"
    items = @(
@{
            itemNo = "PROD001"
    itemName = "Test Product"
            quantity = 2
  itemPrice = 99.99
            availableQuanlity = 100
 }
    )
} | ConvertTo-Json -Depth 3

try {
    Write-Host "Updating basket for user: testuser"
 
    $response = Invoke-RestMethod -Uri "http://localhost:6004/api/baskets" `
    -Method Post `
    -ContentType "application/json" `
        -Body $basket
    
    Write-Host "? Basket updated successfully!" -ForegroundColor Green
    Write-Host "JobId for reminder: $($response.data.jobId)" -ForegroundColor Green
    Write-Host "Total Price: `$$($response.data.totalPrice)" -ForegroundColor Cyan
    
  $jobId = $response.data.jobId
    
    # Check if email was scheduled
    if ($jobId) {
        Write-Host "`n? Email reminder scheduled with JobId: $jobId" -ForegroundColor Green
      Write-Host "? Email will be sent in 10 seconds" -ForegroundColor Yellow
        
    # Wait to see if email gets sent
        Write-Host "`n? Waiting 15 seconds..." -ForegroundColor Cyan
        Start-Sleep -Seconds 15
   
        Write-Host "? Email should have been sent. Check SMTP logs." -ForegroundColor Green
    } else {
        Write-Host "??  No jobId returned - check Basket API logs" -ForegroundColor Yellow
    }
    
    # Test 2: Update basket again (should cancel old job and create new one)
    Write-Host "`n2??  Test: Update basket again (cancels previous reminder)" -ForegroundColor Yellow
    
    $basket2 = @{
      username = "testuser"
        emailAddress = "testuser@example.com"
        items = @(
         @{
         itemNo = "PROD001"
  itemName = "Test Product"
     quantity = 3
     itemPrice = 99.99
       availableQuanlity = 100
            }
        )
    } | ConvertTo-Json -Depth 3
    
    $response2 = Invoke-RestMethod -Uri "http://localhost:6004/api/baskets" `
        -Method Post `
        -ContentType "application/json" `
        -Body $basket2
    
    Write-Host "? Basket updated again!" -ForegroundColor Green
    Write-Host "New JobId: $($response2.data.jobId)" -ForegroundColor Green
    Write-Host "Old job ($jobId) should have been cancelled" -ForegroundColor Yellow
    
    # Test 3: Checkout (should cancel reminder)
    Write-Host "`n3??  Test: Checkout basket (cancels reminder)" -ForegroundColor Yellow
    
    $checkout = @{
   username = "testuser"
        firstName = "Test"
    lastName = "User"
    emailAddress = "testuser@example.com"
        shippingAddress = "123 Test St"
        invoiceAddress = "123 Test St"
  totalPrice = 299.97
 } | ConvertTo-Json
    
    $checkoutResponse = Invoke-RestMethod -Uri "http://localhost:6004/api/baskets/checkout" `
        -Method Post `
        -ContentType "application/json" `
      -Body $checkout
    
    Write-Host "? Checkout successful!" -ForegroundColor Green
    Write-Host "Message: $checkoutResponse" -ForegroundColor Cyan
    
    # Verify basket is deleted
    Start-Sleep -Seconds 2
    
    try {
        $getBasket = Invoke-RestMethod -Uri "http://localhost:6004/api/baskets/testuser" `
            -Method Get
        
        Write-Host "??  Basket still exists after checkout!" -ForegroundColor Yellow
    } catch {
        Write-Host "? Basket was deleted after checkout (as expected)" -ForegroundColor Green
    }
    
} catch {
    Write-Host "? Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "StatusCode: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
}

Write-Host "`n? Flow test completed!" -ForegroundColor Green
Write-Host "?? Check Hangfire Dashboard: http://localhost:6008/jobs" -ForegroundColor Cyan
Write-Host "?? Check email inbox for: testuser@example.com" -ForegroundColor Cyan
