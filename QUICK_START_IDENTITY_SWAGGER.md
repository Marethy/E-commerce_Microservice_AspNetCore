# ?? Quick Start - Identity Server + Swagger OAuth

## ?? Prerequisites
- Docker Desktop running
- PowerShell 7+ (ho?c PowerShell Core)
- Ports available: 6001 (Identity), 6002 (Product), 1436 (IdentityDB), 5434 (ProductDB)

---

## ? Quick Start Commands

### 1. **Start Services**
```powershell
# T? solution root
cd C:\Users\PC\Desktop\mylove\backend_microservices

# Build và start containers
docker-compose build identity-api product-api
docker-compose up -d identitydb productdb elasticsearch
Start-Sleep -Seconds 15  # Wait for databases

docker-compose up -d identity-api product-api

# Check status
docker-compose ps
```

### 2. **Wait for Services to be Ready** (30-60 seconds)
```powershell
# Check logs
docker-compose logs -f identity-api
# Look for: "Now listening on: http://[::]:80"
# Press Ctrl+C to exit

docker-compose logs -f product-api
# Look for: "Application started"
```

### 3. **Run Test Script**
```powershell
# T? solution root
.\test-swagger-oauth.ps1
```

**Expected output:**
```
=== Testing Swagger OAuth Integration ===

1. Checking services...
? Identity Server is running
? Product API is running

2. Fetching OpenID Configuration...
? Discovery document retrieved
   Issuer: http://localhost:6001
   Authorization Endpoint: http://localhost:6001/connect/authorize
   Token Endpoint: http://localhost:6001/connect/token

3. Testing Swagger UI...
? Product API Swagger is accessible
   URL: http://localhost:6002/swagger

4. Testing Client Credentials flow...
? Access token received
   Token type: Bearer
   Expires in: 7200 seconds
   Scope: microservices_api.read microservices_api.write

5. Testing Product API with access token...
? Successfully called Product API with authentication
   Categories count: 3
   Sample category: Electronics

=== Test Complete ===
```

---

## ?? Test Swagger OAuth Flow (Browser)

### **Step-by-Step:**

1. **M? Swagger UI**
   ```
   http://localhost:6002/swagger
   ```

2. **Click nút [Authorize]** (góc trên bên ph?i)

3. **OAuth2 Popup hi?n ra:**
   - Client ID: `microservices_swagger` (auto-filled)
   - Select scopes:
     - ?? `microservices_api.read`
     - ?? `microservices_api.write`
   - Click **[Authorize]**

4. **Redirect to Identity Server:**
   ```
   http://localhost:6001/connect/authorize?
     client_id=microservices_swagger&
     redirect_uri=http://localhost:6002/swagger/oauth2-redirect.html&
     response_type=token&
     scope=microservices_api.read microservices_api.write&
     state=...
   ```

5. **Login Page:**
   - Username: `tedu_admin` (ho?c test user khác)
   - Password: `Admin@123$`
   - Click **[Login]**

6. **Consent Page** (if required):
   - Review permissions
   - Click **[Yes, Allow]**

7. **Redirected back to Swagger:**
   - Token stored in browser
   - Green checkmark appears on [Authorize] button

8. **Test an API:**
   - Expand `GET /api/products`
   - Click **[Try it out]**
   - Click **[Execute]**
   - Should return **200 OK** with data

---

## ?? Troubleshooting

### ? Issue: "ERR_NAME_NOT_RESOLVED" for `identity-api`

**Problem:** Browser không th? resolve Docker internal hostname

**Fix:**
```yaml
# docker-compose.override.yml
identity-api:
  environment:
    - "IdentityServer__BaseUrl=http://localhost:6001"
    - "IdentityServer__IssuerUri=http://localhost:6001"

product-api:
  environment:
    - "ApiConfiguration__IdentityServerBaseUrl=http://localhost:6001"
    - "ApiConfiguration__IssuerUri=http://localhost:6001"
```

**Verify:**
```powershell
docker exec identity-api printenv | grep IdentityServer
# Should show: IdentityServer__BaseUrl=http://localhost:6001
```

---

### ? Issue: "Invalid redirect_uri"

**Fix:** Update Identity Server Config.cs
```csharp
RedirectUris = new List<string>()
{
    "http://localhost:6002/swagger/oauth2-redirect.html", // ? Add this
}
```

**Rebuild:**
```powershell
docker-compose up -d --build identity-api
```

---

### ? Issue: 401 Unauthorized khi call API

**Possible causes:**

1. **Token expired:**
   - Re-authorize trong Swagger
   
2. **Issuer mismatch:**
   ```powershell
   # Check discovery
   curl http://localhost:6001/.well-known/openid-configuration | jq .issuer
   # Should return: "http://localhost:6001"
   
   # Check Product API config
   docker exec product-api printenv | grep IssuerUri
   # Should show: ApiConfiguration__IssuerUri=http://localhost:6001
   ```

3. **Product API không nh?n ???c token:**
   ```powershell
   # Check Product API logs
   docker-compose logs product-api | grep -i "jwt"
   ```

**Fix:**
```powershell
# Restart services
docker-compose restart identity-api product-api

# Clear browser cache
# Re-authorize in Swagger
```

---

### ? Issue: "Unable to load identity configuration"

**Cause:** Product API không th? fetch discovery document t? Identity Server

**Check:**
```powershell
# From Product API container
docker exec product-api curl -v http://localhost:6001/.well-known/openid-configuration

# Should return 200 OK with JSON
```

**If fails:**
```powershell
# Check network connectivity
docker network inspect microservices

# Both containers should be in same network
# identity-api: IPv4Address
# product-api: IPv4Address
```

---

## ?? Verify Setup

### **1. Identity Server Endpoints**
```powershell
# Discovery document
curl http://localhost:6001/.well-known/openid-configuration

# Health check
curl http://localhost:6001/hc

# Login page (browser)
http://localhost:6001/Account/Login
```

### **2. Product API Endpoints**
```powershell
# Health check
curl http://localhost:6002/hc

# Swagger UI (browser)
http://localhost:6002/swagger

# Categories (anonymous - if allowed)
curl http://localhost:6002/api/categories

# Products (requires auth)
curl http://localhost:6002/api/products `
  -H "Authorization: Bearer YOUR_TOKEN"
```

### **3. Database Connectivity**
```powershell
# Identity DB
docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -Q "SELECT name FROM sys.databases WHERE name='TeduIdentity'"

# Product DB
docker exec -it productdb psql -U admin -d ProductDb -c "\dt"
```

---

## ?? Success Criteria

? **Identity Server:**
- Discovery document returns correct issuer: `http://localhost:6001`
- Login page accessible at `http://localhost:6001/Account/Login`
- Token endpoint returns valid JWT tokens

? **Product API:**
- Swagger UI accessible at `http://localhost:6002/swagger`
- [Authorize] button redirects to `http://localhost:6001/connect/authorize`
- After login, token stored and used automatically
- API calls return 200 OK with data

? **Integration:**
- OAuth flow completes without DNS errors
- JWT tokens validated successfully
- Protected endpoints require authentication
- Unauthorized calls return 401

---

## ?? Advanced Configuration

### **Change Token Lifetime**
```csharp
// Config.cs
new Client
{
    ClientId = "microservices_swagger",
    AccessTokenLifetime = 60 * 60 * 8, // 8 hours (default: 2 hours)
}
```

### **Add More Redirect URIs**
```csharp
RedirectUris = new List<string>()
{
    "http://localhost:6000/swagger/oauth2-redirect.html", // API Gateway
    "http://localhost:6002/swagger/oauth2-redirect.html", // Product API
    "http://localhost:6004/swagger/oauth2-redirect.html", // Basket API
}
```

### **Enable Consent Screen**
```csharp
new Client
{
    RequireConsent = true, // Force user approval
}
```

---

## ?? Useful Commands

```powershell
# View all logs
docker-compose logs -f identity-api product-api

# Restart specific service
docker-compose restart identity-api

# Rebuild and restart
docker-compose up -d --build identity-api

# Stop everything
docker-compose down

# Remove volumes (fresh start)
docker-compose down -v

# Check container status
docker-compose ps

# Execute command in container
docker exec -it identity-api bash

# View environment variables
docker exec identity-api printenv | grep Identity
```

---

## ?? You're All Set!

If all tests pass, your Identity Server và Product API integration is working correctly!

**Next Steps:**
1. Add authentication to other APIs (Basket, Order, etc.)
2. Configure CORS for frontend applications
3. Add user registration flow
4. Implement refresh tokens
5. Set up production HTTPS

---

**Need Help?**
- Check logs: `docker-compose logs -f identity-api`
- Run test script: `.\test-swagger-oauth.ps1`
- Verify environment: `docker exec identity-api printenv`
