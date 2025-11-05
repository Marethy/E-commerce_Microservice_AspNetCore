# Test Hangfire API - Scheduled Email
Write-Host "?? Testing Hangfire API..." -ForegroundColor Cyan

# Test 1: Schedule email (10 seconds from now)
Write-Host "`n1??  Test: Schedule Reminder Email" -ForegroundColor Yellow

$scheduleTime = (Get-Date).AddSeconds(10).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")

$body = @{
    email = "test@example.com"
    subject = "Test Reminder Email"
    content = "<h1>Hello from Hangfire!</h1><p>This email was scheduled 10 seconds ago.</p>"
    enqueueAt = $scheduleTime
} | ConvertTo-Json

Write-Host "Scheduling email for: $scheduleTime"

try {
    $response = Invoke-RestMethod -Uri "http://localhost:6008/api/ScheduledJobs/send-reminder-email" `
        -Method Post `
        -ContentType "application/json" `
  -Body $body

    Write-Host "? Email scheduled successfully!" -ForegroundColor Green
    Write-Host "JobId: $response" -ForegroundColor Green
    
    $jobId = $response

    # Wait for email to be sent
    Write-Host "`n? Waiting 15 seconds for email to be sent..." -ForegroundColor Cyan
    Start-Sleep -Seconds 15
    
    Write-Host "? Email should have been sent. Check SMTP logs or email inbox." -ForegroundColor Green
    
    # Test 2: Delete a scheduled job (schedule one for future, then delete it)
    Write-Host "`n2??  Test: Schedule & Delete Job" -ForegroundColor Yellow
 
    $futureTime = (Get-Date).AddMinutes(30).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    $body2 = @{
  email = "delete-test@example.com"
        subject = "This email will be cancelled"
      content = "<p>This should never be sent.</p>"
        enqueueAt = $futureTime
    } | ConvertTo-Json
    
    $response2 = Invoke-RestMethod -Uri "http://localhost:6008/api/ScheduledJobs/send-reminder-email" `
        -Method Post `
        -ContentType "application/json" `
        -Body $body2
 
    $jobId2 = $response2
    Write-Host "? Job scheduled for deletion: $jobId2" -ForegroundColor Green
 
    # Delete it
    Start-Sleep -Seconds 2
    $deleteResponse = Invoke-RestMethod -Uri "http://localhost:6008/api/ScheduledJobs/delete-job/$jobId2" `
        -Method Delete
    
    Write-Host "? Job deleted: $deleteResponse" -ForegroundColor Green
    
} catch {
    Write-Host "? Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response: $($_.Exception.Response)" -ForegroundColor Red
}

Write-Host "`n? All tests completed!" -ForegroundColor Green
Write-Host "?? Check Hangfire Dashboard: http://localhost:6008/jobs" -ForegroundColor Cyan
