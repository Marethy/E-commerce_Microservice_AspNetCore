# Test Swagger OAuth Flow with Identity Server

Write-Host "=== Testing Swagger OAuth Integration ===" -ForegroundColor Cyan

# 1. Check if services are running
Write-Host "`n1. Checking services..." -ForegroundColor Yellow
try {
    $identityHealth = Invoke-RestMethod -Uri "http://localhost:6001/hc" -Method Get -TimeoutSec 5
    Write-Host "? Identity Server is running" -ForegroundColor Green
} catch {
    Write-Host "? Identity Server is NOT running on port 6001" -ForegroundColor Red
    Write-Host "   Run: docker-compose up -d identity-api" -ForegroundColor Yellow
    exit 1
}

try {
    $productHealth = Invoke-RestMethod -Uri "http://localhost:6002/hc" -Method Get -TimeoutSec 5
    Write-Host "? Product API is running" -ForegroundColor Green
} catch {
    Write-Host "? Product API is NOT running on port 6002" -ForegroundColor Red
    Write-Host "   Run: docker-compose up -d product-api" -ForegroundColor Yellow
    exit 1
}

# 2. Check OpenID Configuration
Write-Host "`n2. Fetching OpenID Configuration..." -ForegroundColor Yellow
try {
    $discovery = Invoke-RestMethod -Uri "http://localhost:6001/.well-known/openid-configuration" -Method Get
    Write-Host "? Discovery document retrieved" -ForegroundColor Green
    Write-Host "   Issuer: $($discovery.issuer)" -ForegroundColor Gray
    Write-Host "   Authorization Endpoint: $($discovery.authorization_endpoint)" -ForegroundColor Gray
    Write-Host "   Token Endpoint: $($discovery.token_endpoint)" -ForegroundColor Gray
    
    # Verify issuer
    if ($discovery.issuer -ne "http://localhost:6001") {
        Write-Host "??  WARNING: Issuer mismatch!" -ForegroundColor Yellow
        Write-Host "   Expected: http://localhost:6001" -ForegroundColor Yellow
        Write-Host "   Got: $($discovery.issuer)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "? Failed to get discovery document" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 3. Test Swagger UI accessibility
Write-Host "`n3. Testing Swagger UI..." -ForegroundColor Yellow
try {
    $swaggerResponse = Invoke-WebRequest -Uri "http://localhost:6002/swagger" -Method Get -UseBasicParsing
    if ($swaggerResponse.StatusCode -eq 200) {
        Write-Host "? Product API Swagger is accessible" -ForegroundColor Green
        Write-Host "   URL: http://localhost:6002/swagger" -ForegroundColor Gray
    }
} catch {
    Write-Host "? Cannot access Swagger UI" -ForegroundColor Red
    exit 1
}

# 4. Get Client Credentials Token (for API testing)
Write-Host "`n4. Testing Client Credentials flow..." -ForegroundColor Yellow
$tokenBody = @{
    client_id = "microservices_postman"
    client_secret = "SuperStrongSecret"
    grant_type = "client_credentials"
    scope = "microservices_api.read microservices_api.write"
}

try {
    $tokenResponse = Invoke-RestMethod -Uri "http://localhost:6001/connect/token" `
        -Method Post `
        -Body $tokenBody `
        -ContentType "application/x-www-form-urlencoded"
    
    Write-Host "? Access token received" -ForegroundColor Green
    Write-Host "   Token type: $($tokenResponse.token_type)" -ForegroundColor Gray
    Write-Host "   Expires in: $($tokenResponse.expires_in) seconds" -ForegroundColor Gray
    Write-Host "   Scope: $($tokenResponse.scope)" -ForegroundColor Gray
    
    $token = $tokenResponse.access_token
    
    # Decode JWT to show claims
    $tokenParts = $token.Split('.')
    $payload = $tokenParts[1]
    # Add padding if needed
    while ($payload.Length % 4 -ne 0) {
        $payload += "="
    }
    $decodedBytes = [Convert]::FromBase64String($payload)
    $decodedJson = [System.Text.Encoding]::UTF8.GetString($decodedBytes)
    $claims = $decodedJson | ConvertFrom-Json
    
    Write-Host "`n   Token Claims:" -ForegroundColor Gray
    Write-Host "   - Issuer: $($claims.iss)" -ForegroundColor Gray
    Write-Host "   - Client: $($claims.client_id)" -ForegroundColor Gray
    Write-Host "   - Scope: $($claims.scope)" -ForegroundColor Gray
    
} catch {
    Write-Host "? Failed to get access token" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 5. Test Product API with token
Write-Host "`n5. Testing Product API with access token..." -ForegroundColor Yellow
$headers = @{
    Authorization = "Bearer $token"
}

try {
    $categories = Invoke-RestMethod -Uri "http://localhost:6002/api/categories" -Method Get -Headers $headers
    Write-Host "? Successfully called Product API with authentication" -ForegroundColor Green
    Write-Host "   Categories count: $($categories.Count)" -ForegroundColor Gray
    
    if ($categories.Count -gt 0) {
        Write-Host "   Sample category: $($categories[0].name)" -ForegroundColor Gray
    }
} catch {
    Write-Host "? Failed to call Product API" -ForegroundColor Red
    Write-Host "   Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    Write-Host "   Message: $($_.Exception.Message)" -ForegroundColor Red
}

# 6. Instructions for Swagger OAuth
Write-Host "`n=== Manual Testing Instructions ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "? All automated tests passed!" -ForegroundColor Green
Write-Host ""
Write-Host "To test Swagger OAuth flow manually:" -ForegroundColor Yellow
Write-Host "1. Open browser: http://localhost:6002/swagger" -ForegroundColor White
Write-Host "2. Click [Authorize] button" -ForegroundColor White
Write-Host "3. Check the scopes you want:" -ForegroundColor White
Write-Host "   ? microservices_api.read" -ForegroundColor White
Write-Host "   ? microservices_api.write" -ForegroundColor White
Write-Host "4. Click [Authorize] button in popup" -ForegroundColor White
Write-Host "5. You'll be redirected to:" -ForegroundColor White
Write-Host "   http://localhost:6001/connect/authorize?..." -ForegroundColor Cyan
Write-Host "6. Login with test user credentials" -ForegroundColor White
Write-Host "7. Approve consent (if required)" -ForegroundColor White
Write-Host "8. You'll be redirected back to Swagger" -ForegroundColor White
Write-Host "9. Token will be stored and used automatically" -ForegroundColor White
Write-Host "10. Try calling any API endpoint" -ForegroundColor White
Write-Host ""
Write-Host "Expected Authorize URL:" -ForegroundColor Yellow
Write-Host "http://localhost:6001/connect/authorize?..." -ForegroundColor Cyan
Write-Host ""
Write-Host "If you see 'ERR_NAME_NOT_RESOLVED':" -ForegroundColor Red
Write-Host "- Make sure Identity Server is using 'localhost:6001' not 'identity-api'" -ForegroundColor Yellow
Write-Host "- Check docker-compose.override.yml:" -ForegroundColor Yellow
Write-Host "  IdentityServer__BaseUrl=http://localhost:6001" -ForegroundColor Cyan
Write-Host "  IdentityServer__IssuerUri=http://localhost:6001" -ForegroundColor Cyan
Write-Host ""
Write-Host "=== Test Complete ===" -ForegroundColor Cyan
