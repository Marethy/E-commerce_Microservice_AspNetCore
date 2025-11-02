# ? Identity Server + Product API - Setup Verification Checklist

## ?? Pre-Flight Checklist

### **Configuration Files**

- [ ] `docker-compose.override.yml` updated:
  ```yaml
  identity-api:
    environment:
      - "IdentityServer__BaseUrl=http://localhost:6001"
      - "IdentityServer__IssuerUri=http://localhost:6001"
  
  product-api:
    environment:
      - "ApiConfiguration__IdentityServerBaseUrl=http://localhost:6001"
      - "ApiConfiguration__IssuerUri=http://localhost:6001"
  ```

- [ ] `Config.cs` includes Swagger redirect URIs:
  ```csharp
  RedirectUris = new List<string>()
  {
      "http://localhost:6002/swagger/oauth2-redirect.html",
  }
  ```

- [ ] Port mapping correct:
  - Identity: `6001:80`
  - Product: `6002:80`

---

## ?? Docker Services

### **Start Services:**
```powershell
docker-compose up -d identitydb productdb elasticsearch
Start-Sleep -Seconds 15
docker-compose up -d identity-api product-api
Start-Sleep -Seconds 30
```

### **Verify Containers Running:**
```powershell
docker-compose ps
```

**Expected output:**
```
NAME            IMAGE                          STATUS
identitydb      mssql/server:2022-latest       Up (healthy)
productdb       postgres:alpine                Up (healthy)
identity-api    identity-api:latest            Up
product-api     product-api:latest             Up
elasticsearch   elasticsearch:7.17.2           Up
```

**Checklist:**
- [ ] identitydb: `Up` status, port 1436
- [ ] productdb: `Up` status, port 5434
- [ ] identity-api: `Up` status, port 6001
- [ ] product-api: `Up` status, port 6002
- [ ] elasticsearch: `Up` status, port 9200

---

## ?? Service Health Checks

### **Identity Server:**
```powershell
curl http://localhost:6001/hc
```
**Expected:** `{"status":"Healthy",...}`

- [ ] Health check returns `Healthy`
- [ ] Response time < 1 second

### **Product API:**
```powershell
curl http://localhost:6002/hc
```
**Expected:** `{"status":"Healthy",...}`

- [ ] Health check returns `Healthy`
- [ ] Response time < 1 second

---

## ?? Identity Server Configuration

### **1. Discovery Document:**
```powershell
curl http://localhost:6001/.well-known/openid-configuration | jq
```

**Verify:**
- [ ] `"issuer": "http://localhost:6001"`
- [ ] `"authorization_endpoint": "http://localhost:6001/connect/authorize"`
- [ ] `"token_endpoint": "http://localhost:6001/connect/token"`
- [ ] `"jwks_uri": "http://localhost:6001/.well-known/openid-configuration/jwks"`

### **2. JWKS Endpoint:**
```powershell
curl http://localhost:6001/.well-known/openid-configuration/jwks | jq
```

**Verify:**
- [ ] Returns JSON with `"keys": [...]`
- [ ] At least 1 key present
- [ ] Key has `"kty": "RSA"`

### **3. Login Page:**
```
http://localhost:6001/Account/Login
```

**Verify (in browser):**
- [ ] Page loads without errors
- [ ] Username and password fields present
- [ ] No 404 or 500 errors

### **4. Get Token (Client Credentials):**
```powershell
$response = Invoke-RestMethod -Uri "http://localhost:6001/connect/token" `
  -Method Post `
  -Body @{
    client_id = "microservices_postman"
    client_secret = "SuperStrongSecret"
    grant_type = "client_credentials"
    scope = "microservices_api.read microservices_api.write"
  } `
  -ContentType "application/x-www-form-urlencoded"

$response.access_token
```

**Verify:**
- [ ] Request returns 200 OK
- [ ] Response contains `access_token`
- [ ] Response contains `expires_in`
- [ ] `token_type` is `Bearer`

---

## ?? Product API Configuration

### **1. Swagger UI:**
```
http://localhost:6002/swagger
```

**Verify (in browser):**
- [ ] Swagger UI loads successfully
- [ ] API endpoints visible
- [ ] **[Authorize]** button present (top right)

### **2. Swagger JSON:**
```powershell
curl http://localhost:6002/swagger/v1/swagger.json | jq
```

**Verify:**
- [ ] JSON document returns
- [ ] Contains `securityDefinitions` or `components.securitySchemes`
- [ ] OAuth2 configuration present

### **3. Categories Endpoint (Anonymous):**
```powershell
curl http://localhost:6002/api/categories
```

**Verify:**
- [ ] Returns array of categories (if anonymous allowed)
- [ ] OR returns 401 Unauthorized (if auth required)

### **4. Products Endpoint (Authenticated):**
```powershell
$token = "YOUR_TOKEN_FROM_STEP_4_ABOVE"
curl http://localhost:6002/api/products `
  -H "Authorization: Bearer $token"
```

**Verify:**
- [ ] Returns 200 OK with products array
- [ ] No 401 Unauthorized error
- [ ] Token is accepted

---

## ?? OAuth Flow Test (Manual)

### **Step 1: Open Swagger**
```
http://localhost:6002/swagger
```
- [ ] Page loads

### **Step 2: Click [Authorize]**
- [ ] OAuth2 popup appears
- [ ] `microservices_api.read` scope visible
- [ ] `microservices_api.write` scope visible

### **Step 3: Check Scopes & Click [Authorize]**
- [ ] Both scopes checked
- [ ] Click redirects to Identity Server

### **Step 4: Verify Redirect URL**
**Expected URL pattern:**
```
http://localhost:6001/connect/authorize?
  client_id=microservices_swagger&
  redirect_uri=http://localhost:6002/swagger/oauth2-redirect.html&
  response_type=token&
  scope=microservices_api.read%20microservices_api.write&
  state=...
```

**Verify:**
- [ ] URL uses `http://localhost:6001` (NOT `identity-api`)
- [ ] `redirect_uri` includes `localhost:6002`
- [ ] No DNS errors
- [ ] No certificate errors

### **Step 5: Login Page**
- [ ] Login form appears
- [ ] Username field present
- [ ] Password field present
- [ ] No error messages

### **Step 6: Login with Test User**
**Credentials:**
- Username: `tedu_admin`
- Password: `Admin@123$`

- [ ] Login successful
- [ ] No "Invalid credentials" error

### **Step 7: Consent Page (if required)**
- [ ] Consent page appears
- [ ] Requested scopes displayed
- [ ] **[Yes, Allow]** button works

### **Step 8: Redirect Back to Swagger**
- [ ] Redirected to: `http://localhost:6002/swagger/oauth2-redirect.html`
- [ ] Then auto-redirected back to Swagger UI
- [ ] **[Authorize]** button shows green checkmark ?
- [ ] No errors in browser console

### **Step 9: Call API with Token**
**Test endpoint:** `GET /api/products`
- [ ] Click **[Try it out]**
- [ ] Click **[Execute]**
- [ ] Request includes `Authorization: Bearer ...` header
- [ ] Response: **200 OK**
- [ ] Products data returned

---

## ??? Database Verification

### **Identity Database:**
```powershell
docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -Q "USE TeduIdentity; SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES"
```

**Verify:**
- [ ] Database `TeduIdentity` exists
- [ ] Table `AspNetUsers` exists
- [ ] Table `Clients` exists (IdentityServer config)
- [ ] Table `PersistedGrants` exists (IdentityServer operational)

**Check seed users:**
```powershell
docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -Q "USE TeduIdentity; SELECT UserName, Email FROM AspNetUsers"
```

- [ ] At least 1 user exists
- [ ] `tedu_admin` user present (or your test user)

### **Product Database:**
```powershell
docker exec -it productdb psql -U admin -d ProductDb -c "\dt"
```

**Verify:**
- [ ] Database `ProductDb` exists
- [ ] Table `categories` exists
- [ ] Table `products` exists

**Check seed data:**
```powershell
docker exec -it productdb psql -U admin -d ProductDb -c "SELECT * FROM categories LIMIT 5"
```

- [ ] Categories exist
- [ ] At least 3 categories

---

## ?? Automated Test Script

```powershell
.\test-swagger-oauth.ps1
```

**All checks should pass:**
- [ ] ? Identity Server is running
- [ ] ? Product API is running
- [ ] ? Discovery document retrieved
- [ ] ? Product API Swagger is accessible
- [ ] ? Access token received
- [ ] ? Successfully called Product API with authentication

---

## ?? Security Verification

### **JWT Token Validation:**
1. Get token from Identity Server
2. Decode at https://jwt.io

**Verify claims:**
- [ ] `"iss": "http://localhost:6001"` (Issuer)
- [ ] `"aud": "microservices_api"` (Audience)
- [ ] `"scope"` includes requested scopes
- [ ] `"exp"` is future timestamp (not expired)
- [ ] `"nbf"` (Not Before) is past/current timestamp

### **Token Expiration:**
```powershell
# Wait for AccessTokenLifetime to pass (default: 2 hours)
# Or set to 60 seconds for testing:
# AccessTokenLifetime = 60

# Try to use expired token
curl http://localhost:6002/api/products `
  -H "Authorization: Bearer EXPIRED_TOKEN"
```

**Verify:**
- [ ] Returns 401 Unauthorized
- [ ] Error message mentions "token expired"

### **Invalid Token:**
```powershell
curl http://localhost:6002/api/products `
  -H "Authorization: Bearer INVALID_TOKEN_123"
```

**Verify:**
- [ ] Returns 401 Unauthorized
- [ ] Error message mentions "invalid token"

---

## ?? Monitoring & Logs

### **Check Logs:**
```powershell
# Identity Server logs
docker-compose logs identity-api | tail -n 50

# Product API logs
docker-compose logs product-api | tail -n 50
```

**Look for:**
- [ ] No ERROR level messages
- [ ] No unhandled exceptions
- [ ] JWT validation success messages
- [ ] Token introspection logs (if enabled)

### **Elasticsearch (Optional):**
```
http://localhost:5601 (Kibana)
```

**Verify:**
- [ ] Kibana accessible
- [ ] Logs from `identity-api` indexed
- [ ] Logs from `product-api` indexed
- [ ] Search works: `application:"identity-api"`

---

## ?? Final Checklist

### **Core Functionality:**
- [ ] Identity Server issues valid JWT tokens
- [ ] Product API validates tokens correctly
- [ ] Swagger OAuth flow works end-to-end
- [ ] Protected endpoints require authentication
- [ ] Anonymous endpoints (if any) work without auth

### **URL Configuration:**
- [ ] All URLs use `localhost:600X` (not internal Docker names)
- [ ] Issuer URI matches between Identity & Product API
- [ ] Redirect URIs registered in Identity Server config
- [ ] CORS allows Swagger origin

### **Error Handling:**
- [ ] Expired tokens return 401
- [ ] Invalid tokens return 401
- [ ] Missing tokens return 401
- [ ] Unauthorized access returns 403 (if applicable)

### **Performance:**
- [ ] Token generation < 500ms
- [ ] Token validation < 100ms
- [ ] API response time < 1 second
- [ ] No memory leaks in long-running containers

---

## ? Success Criteria

**All of the following must be TRUE:**

? Automated test script passes all checks
? Manual Swagger OAuth flow completes without errors
? Browser never shows DNS resolution errors
? Protected API endpoints return 200 OK with valid token
? Protected API endpoints return 401 without token
? Logs show no errors or exceptions
? Databases contain expected schema and seed data

---

## ?? If Any Check Fails

### **Quick Fixes:**

1. **Restart services:**
   ```powershell
   docker-compose restart identity-api product-api
   ```

2. **Rebuild images:**
   ```powershell
   docker-compose up -d --build identity-api product-api
   ```

3. **Reset databases:**
   ```powershell
   docker-compose down -v
   docker-compose up -d
   ```

4. **Clear browser cache:**
   - Open Dev Tools (F12)
   - Right-click Refresh ? Empty Cache and Hard Reload

5. **Check environment variables:**
   ```powershell
   docker exec identity-api printenv | grep IdentityServer
   docker exec product-api printenv | grep ApiConfiguration
   ```

### **Still not working?**
- Review `QUICK_START_IDENTITY_SWAGGER.md`
- Check logs: `docker-compose logs -f`
- Verify network: `docker network inspect microservices`
- Test from within container: `docker exec identity-api curl localhost/.well-known/openid-configuration`

---

**Date completed:** ___________

**Tested by:** ___________

**All checks passed:** [ ] YES  [ ] NO

**Notes:**
_________________________________________________________________
_________________________________________________________________
_________________________________________________________________
