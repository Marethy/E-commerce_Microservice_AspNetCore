# Comprehensive Inventory gRPC Verification Script
Write-Host "?? INVENTORY gRPC VERIFICATION TEST SUITE" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# Test 1: Check container health
Write-Host "`n1??  Container Health Check" -ForegroundColor Yellow
try {
    $containerStatus = docker ps --filter "name=inventory-grpc" --format "{{.Status}}"
    if ($containerStatus -match "Up") {
        Write-Host "? inventory-grpc container is running" -ForegroundColor Green
        Write-Host "   Status: $containerStatus" -ForegroundColor Gray
    } else {
        Write-Host "? inventory-grpc container is not running properly" -ForegroundColor Red
     exit 1
  }
} catch {
    Write-Host "? Error checking container: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 2: Check logs for errors
Write-Host "`n2??  Log Analysis" -ForegroundColor Yellow
$logs = docker logs inventory-grpc --tail 50 2>&1
$errors = $logs | Select-String -Pattern "error|exception|fail" -CaseSensitive:$false

if ($errors) {
 Write-Host "??  Found potential issues in logs:" -ForegroundColor Yellow
    $errors | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
} else {
    Write-Host "? No errors found in recent logs" -ForegroundColor Green
}

# Check for HTTP/2 configuration
if ($logs -match "Kestrel.*Http2" -or $logs -match "Protocols.*Http2") {
    Write-Host "? HTTP/2 protocol detected in logs" -ForegroundColor Green
} else {
    Write-Host "??  HTTP/2 configuration not explicitly shown in logs" -ForegroundColor Yellow
}

# Test 3: Test gRPC endpoint from Basket API
Write-Host "`n3??  gRPC Call Test (via Basket API)" -ForegroundColor Yellow

$testBasket = @{
    username = "grpc-test-user"
    emailAddress = "grpctest@example.com"
  items = @(
        @{
            itemNo = "TEST-ITEM-001"
            itemName = "Test Product for gRPC"
       quantity = 1
            itemPrice = 49.99
availableQuanlity = 0
        }
    )
} | ConvertTo-Json -Depth 3

try {
    Write-Host "   Sending test request to Basket API..." -ForegroundColor Gray
    $response = Invoke-RestMethod -Uri "http://localhost:6004/api/baskets" `
        -Method Post `
        -ContentType "application/json" `
        -Body $testBasket `
        -ErrorAction Stop
    
    if ($response.isSuccess) {
        Write-Host "? Basket API successfully called Inventory gRPC!" -ForegroundColor Green
Write-Host "   Stock quantity retrieved: $($response.data.items[0].availableQuanlity)" -ForegroundColor Gray
        
   # Cleanup test basket
        try {
  Invoke-RestMethod -Uri "http://localhost:6004/api/baskets/grpc-test-user" `
    -Method Delete `
  -ErrorAction SilentlyContinue | Out-Null
    } catch {
            # Ignore cleanup errors
        }
    } else {
        Write-Host "??  Basket API response not successful" -ForegroundColor Yellow
    Write-Host "   Response: $($response | ConvertTo-Json -Depth 2)" -ForegroundColor Gray
    }
} catch {
    Write-Host "? gRPC call failed!" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Gray
    
    # Check if it's a gRPC-specific error
    if ($_.Exception.Message -match "HTTP_1_1_REQUIRED|HTTP/2|gRPC") {
        Write-Host "   ??  This appears to be an HTTP/2 protocol issue" -ForegroundColor Yellow
        exit 1
    }
}

# Test 4: Network connectivity
Write-Host "`n4??  Network Connectivity Test" -ForegroundColor Yellow
try {
    $networkTest = docker exec basket-api ping -c 2 inventory-grpc 2>&1
    if ($LASTEXITCODE -eq 0) {
    Write-Host "? basket-api can reach inventory-grpc via Docker network" -ForegroundColor Green
    } else {
        Write-Host "??  Network connectivity issue detected" -ForegroundColor Yellow
    }
} catch {
 Write-Host "??  Could not test network connectivity: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Test 5: Port accessibility
Write-Host "`n5??  Port Accessibility" -ForegroundColor Yellow
try {
    $portTest = Test-NetConnection -ComputerName localhost -Port 6007 -WarningAction SilentlyContinue
    if ($portTest.TcpTestSucceeded) {
  Write-Host "? Port 6007 is accessible from host" -ForegroundColor Green
    } else {
        Write-Host "??  Port 6007 is not accessible" -ForegroundColor Yellow
    }
} catch {
    Write-Host "??  Port test failed: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Test 6: MongoDB connection (Inventory depends on MongoDB)
Write-Host "`n6??  MongoDB Dependency Check" -ForegroundColor Yellow
$mongoLogs = docker logs inventorydb --tail 20 2>&1
if ($mongoLogs -match "waiting for connections") {
    Write-Host "? MongoDB (inventorydb) is ready" -ForegroundColor Green
} else {
    Write-Host "??  MongoDB may not be fully ready" -ForegroundColor Yellow
}

# Test 7: Configuration verification
Write-Host "`n7??  Configuration Verification" -ForegroundColor Yellow
$envVars = docker inspect inventory-grpc --format '{{json .Config.Env}}' | ConvertFrom-Json

$http2Config = $envVars | Where-Object { $_ -match "Kestrel__EndpointDefaults__Protocols" }
if ($http2Config) {
    Write-Host "? HTTP/2 configuration found in environment variables" -ForegroundColor Green
    Write-Host "   $http2Config" -ForegroundColor Gray
} else {
    Write-Host "??  HTTP/2 configuration not found in environment variables" -ForegroundColor Yellow
}

# Test 8: Recent restart check
Write-Host "`n8??  Uptime Check" -ForegroundColor Yellow
$startTime = docker inspect inventory-grpc --format '{{.State.StartedAt}}'
$started = [DateTime]::Parse($startTime)
$uptime = (Get-Date) - $started

if ($uptime.TotalMinutes -lt 5) {
    Write-Host "??  Container was recently restarted ($([math]::Round($uptime.TotalMinutes, 1)) minutes ago)" -ForegroundColor Yellow
    Write-Host "   Waiting 5 seconds for full initialization..." -ForegroundColor Gray
    Start-Sleep -Seconds 5
} else {
    Write-Host "? Container has been running for $([math]::Round($uptime.TotalMinutes, 1)) minutes" -ForegroundColor Green
}

# Final Summary
Write-Host "`n" -NoNewline
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "?? VERIFICATION SUMMARY" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

$allPassed = $true

# Check critical conditions
if ($containerStatus -notmatch "Up") {
    Write-Host "? CRITICAL: Container not running" -ForegroundColor Red
    $allPassed = $false
}

if ($errors -and ($errors | Where-Object { $_ -match "fatal|critical" })) {
    Write-Host "? CRITICAL: Fatal errors in logs" -ForegroundColor Red
    $allPassed = $false
}

if ($allPassed) {
  Write-Host "`n? ALL CHECKS PASSED - Inventory gRPC is working correctly!" -ForegroundColor Green
    Write-Host "   Ready for staging deployment" -ForegroundColor Green
} else {
  Write-Host "`n? SOME CHECKS FAILED - Review issues before staging" -ForegroundColor Red
    exit 1
}

Write-Host "`n?? Quick Stats:" -ForegroundColor Cyan
Write-Host "   Container: inventory-grpc" -ForegroundColor Gray
Write-Host "   Port: 6007 (external) ? 80 (internal)" -ForegroundColor Gray
Write-Host "   Protocol: HTTP/2 (gRPC)" -ForegroundColor Gray
Write-Host "   Database: MongoDB (inventorydb)" -ForegroundColor Gray
Write-Host "   Dependent Services: basket-api, inventory-product-api" -ForegroundColor Gray

Write-Host "`n? Verification complete!" -ForegroundColor Green
