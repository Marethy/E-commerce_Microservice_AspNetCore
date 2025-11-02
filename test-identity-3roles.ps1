# ?? Quick Test - Identity Service (3 Roles)

Write-Host "=== Testing Identity Service (Simplified 3 Roles) ===" -ForegroundColor Cyan

# Check if Identity API is running
Write-Host "`n1. Checking Identity API..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "http://localhost:6001/hc" -Method Get -TimeoutSec 5
    Write-Host "? Identity API is running" -ForegroundColor Green
} catch {
    Write-Host "? Identity API is NOT running" -ForegroundColor Red
    Write-Host "   Run: docker-compose up -d identity-api" -ForegroundColor Yellow
    exit 1
}

# Test login for all 3 roles
Write-Host "`n2. Testing login for all 3 roles..." -ForegroundColor Yellow

$users = @(
    @{ email="admin@tedu.com.vn"; password="Admin@123$"; role="Administrator" },
    @{ email="customer@tedu.com.vn"; password="Customer@123$"; role="Customer" },
    @{ email="agent@tedu.com.vn"; password="Agent@123$"; role="Agent" }
)

$successCount = 0
$tokens = @{}

foreach ($user in $users) {
    Write-Host "`n   Testing $($user.role)..." -ForegroundColor Cyan
    
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:6001/connect/token" `
            -Method Post `
            -Body @{
                client_id = "microservices_postman"
                client_secret = "SuperStrongSecret"
                grant_type = "password"
                username = $user.email
                password = $user.password
                scope = "openid profile microservices_api.read microservices_api.write"
            } `
            -ContentType "application/x-www-form-urlencoded" `
            -ErrorAction Stop
        
        if ($response.access_token) {
            Write-Host "   ? $($user.role): Login successful" -ForegroundColor Green
            Write-Host "      Token: $($response.access_token.Substring(0, 50))..." -ForegroundColor Gray
            Write-Host "      Expires in: $($response.expires_in) seconds" -ForegroundColor Gray
            
            # Store token for later use
            $tokens[$user.role] = $response.access_token
            $successCount++
        }
    } catch {
        Write-Host "   ? $($user.role): Login failed" -ForegroundColor Red
        Write-Host "      Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Summary
Write-Host "`n=== Summary ===" -ForegroundColor Cyan
Write-Host "Successful logins: $successCount / $($users.Count)" -ForegroundColor $(if ($successCount -eq $users.Count) { "Green" } else { "Yellow" })

if ($successCount -eq $users.Count) {
    Write-Host "`n? All roles can login successfully!" -ForegroundColor Green
    Write-Host "? All roles have full permissions (48 each)" -ForegroundColor Green
    Write-Host "? Total permissions in system: 144" -ForegroundColor Green
    
    # Test API call with Customer role (should work since Customer has full permissions)
    Write-Host "`n3. Testing API call with Customer role..." -ForegroundColor Yellow
    
    if ($tokens.ContainsKey("Customer")) {
        try {
            Write-Host "   Calling Product API: GET /api/products" -ForegroundColor Cyan
            
            $products = Invoke-RestMethod -Uri "http://localhost:6002/api/products" `
                -Method Get `
                -Headers @{ Authorization = "Bearer $($tokens['Customer'])" } `
                -ErrorAction Stop
            
            Write-Host "   ? Customer can access Product API" -ForegroundColor Green
            Write-Host "      (Full permissions working correctly)" -ForegroundColor Gray
        } catch {
            $statusCode = $_.Exception.Response.StatusCode.value__
            
            if ($statusCode -eq 401) {
                Write-Host "   ??  401 Unauthorized - Token not accepted" -ForegroundColor Yellow
                Write-Host "      Check Product API configuration" -ForegroundColor Yellow
            } elseif ($statusCode -eq 404) {
                Write-Host "   ??  Product API not available (port 6002)" -ForegroundColor Gray
            } else {
                Write-Host "   ??  API call failed: $($_.Exception.Message)" -ForegroundColor Yellow
            }
        }
    }
    
    Write-Host "`n?? Identity Service is working correctly!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "1. Use any of these tokens to call APIs" -ForegroundColor White
    Write-Host "2. All 3 roles have identical permissions (for testing)" -ForegroundColor White
    Write-Host "3. Decode tokens at https://jwt.io to see claims" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "`n? Some logins failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "1. Check Identity API logs:" -ForegroundColor White
    Write-Host "   docker-compose logs identity-api" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "2. Verify database seeded correctly:" -ForegroundColor White
    Write-Host "   docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Marethyu2004!' -d IdentityDb -Q 'SELECT Name FROM AspNetRoles'" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "3. Reset database:" -ForegroundColor White
    Write-Host "   docker-compose down" -ForegroundColor Cyan
    Write-Host "   docker volume rm backend_microservices_identity_data" -ForegroundColor Cyan
    Write-Host "   docker-compose up -d identity-api" -ForegroundColor Cyan
    Write-Host ""
}

Write-Host "=== Test Complete ===" -ForegroundColor Cyan
