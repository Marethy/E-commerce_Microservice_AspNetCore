# ?? Identity Service - Roles & Permissions (Simplified for Testing)

## ?? Database: `IdentityDb`

### **Overview**
Identity Service s? d?ng **Role-Based Access Control (RBAC)** v?i **Function-Command Permission Model**.

**? SIMPLIFIED FOR TESTING:** All 3 roles have **FULL PERMISSIONS** to make development and testing easier.

---

## ?? **ROLES**

### **1. Administrator** ???
**Full system access** - Qu?n tr? viên h? th?ng
- ? **ALL permissions on ALL functions** (48 permissions)
- ? Full CRUD on PRODUCT, ORDER, CUSTOMER, INVENTORY, BASKET, PAYMENT, SHIPPING, REPORTING

**Default User:**
- Email: `admin@tedu.com.vn`
- Password: `Admin@123$`

**Use Cases:**
- System administration
- User management
- All business operations
- System configuration

---

### **2. Customer** ??
**Full access (for testing)** - Khách hàng
- ? **ALL permissions on ALL functions** (48 permissions)
- ? Full CRUD on PRODUCT, ORDER, CUSTOMER, INVENTORY, BASKET, PAYMENT, SHIPPING, REPORTING

**Default User:**
- Email: `customer@tedu.com.vn`
- Password: `Customer@123$`

**Use Cases (Production will be restricted):**
- Browse products ?
- Manage shopping cart ?
- Place orders ?
- View order history ?
- Manage profile ?
- **(Testing only)** Full access to all operations

---

### **3. Agent** ??
**Full access (for testing)** - Nhân viên
- ? **ALL permissions on ALL functions** (48 permissions)
- ? Full CRUD on PRODUCT, ORDER, CUSTOMER, INVENTORY, BASKET, PAYMENT, SHIPPING, REPORTING

**Default User:**
- Email: `agent@tedu.com.vn`
- Password: `Agent@123$`

**Use Cases (Combines Sales, Support, Warehouse):**
- Customer management ?
- Order processing ?
- Inventory management ?
- Payment processing ?
- Shipping operations ?
- Report generation ?

---

## ?? **FUNCTIONS & COMMANDS**

### **Functions:**
```csharp
public enum FunctionCode
{
    PRODUCT,      // Product catalog
    ORDER,        // Order management
    CUSTOMER,     // Customer management
    INVENTORY,    // Inventory operations
    BASKET,       // Shopping cart
    PAYMENT,      // Payment processing
    SHIPPING,     // Shipping management
    REPORTING     // Reports and analytics
}
```

### **Commands:**
```csharp
public enum CommandCode
{
    CREATE,       // Create new entity
    UPDATE,       // Update existing entity
    DELETE,       // Delete entity
    VIEW,         // Read/View entity
    IMPORT,       // Import data
    EXPORT        // Export data
}
```

---

## ?? **PERMISSION MATRIX**

### **Simplified Matrix (All roles have full access):**

| Role | PRODUCT | ORDER | CUSTOMER | INVENTORY | BASKET | PAYMENT | SHIPPING | REPORTING |
|------|---------|-------|----------|-----------|--------|---------|----------|-----------|
| **Administrator** | ? | ? | ? | ? | ? | ? | ? | ? |
| **Customer** | ? | ? | ? | ? | ? | ? | ? | ? |
| **Agent** | ? | ? | ? | ? | ? | ? | ? | ? |

**Legend:**
- ? **Full Access** (CREATE, UPDATE, DELETE, VIEW, IMPORT, EXPORT)

---

## ?? **DETAILED PERMISSIONS**

### **All Roles (Administrator, Customer, Agent):**
```sql
-- All functions, all commands (48 permissions each)
PRODUCT: CREATE, UPDATE, DELETE, VIEW, IMPORT, EXPORT
ORDER: CREATE, UPDATE, DELETE, VIEW, IMPORT, EXPORT
CUSTOMER: CREATE, UPDATE, DELETE, VIEW, IMPORT, EXPORT
INVENTORY: CREATE, UPDATE, DELETE, VIEW, IMPORT, EXPORT
BASKET: CREATE, UPDATE, DELETE, VIEW, IMPORT, EXPORT
PAYMENT: CREATE, UPDATE, DELETE, VIEW, IMPORT, EXPORT
SHIPPING: CREATE, UPDATE, DELETE, VIEW, IMPORT, EXPORT
REPORTING: CREATE, UPDATE, DELETE, VIEW, IMPORT, EXPORT
```

**Total Permissions per Role:** 48  
**Total Permissions in System:** 144 (48 × 3 roles)

---

## ??? **USAGE IN CODE**

### **1. API Authorization:**
```csharp
[HttpPost]
[ClaimRequirement(FunctionCode.PRODUCT, CommandCode.CREATE)]
public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto productDto)
{
    // All 3 roles (Administrator, Customer, Agent) can access
    // No restrictions for testing purposes
}

[HttpGet]
[ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
public async Task<IActionResult> GetProducts()
{
    // All 3 roles can view products
}
```

### **2. Check Permission in Code:**
```csharp
// Get user permissions from token claims
var permissions = User.Claims
    .Where(c => c.Type == SystemConstants.Claims.Permissions)
    .Select(c => c.Value)
    .ToList();

// All users will have all permissions (for testing)
bool canCreateProduct = permissions.Contains("PRODUCT.CREATE"); // Always true

// Get role
var role = User.FindFirst(SystemConstants.Claims.Roles)?.Value;
// Returns: "Administrator", "Customer", or "Agent"
```

### **3. Get Token with Permissions:**
```powershell
# Login as Admin
curl -X POST http://localhost:6001/connect/token `
  -d "client_id=microservices_postman" `
  -d "client_secret=SuperStrongSecret" `
  -d "grant_type=password" `
  -d "username=admin@tedu.com.vn" `
  -d "password=Admin@123$" `
  -d "scope=openid profile microservices_api.read microservices_api.write"

# Login as Customer
curl -X POST http://localhost:6001/connect/token `
  -d "client_id=microservices_postman" `
  -d "client_secret=SuperStrongSecret" `
  -d "grant_type=password" `
  -d "username=customer@tedu.com.vn" `
  -d "password=Customer@123$" `
  -d "scope=openid profile microservices_api.read microservices_api.write"

# Login as Agent
curl -X POST http://localhost:6001/connect/token `
  -d "client_id=microservices_postman" `
  -d "client_secret=SuperStrongSecret" `
  -d "grant_type=password" `
  -d "username=agent@tedu.com.vn" `
  -d "password=Agent@123$" `
  -d "scope=openid profile microservices_api.read microservices_api.write"

# All tokens will include FULL permissions
```

---

## ?? **DATABASE SCHEMA**

### **AspNetRoles Table:**
```sql
RoleId (PK)  | RoleName
-------------|------------------
guid-1       | Administrator
guid-2       | Customer
guid-3       | Agent
```

### **Permissions Table:**
```sql
Function (PK) | Command (PK) | RoleId (FK)
--------------|--------------|-------------
PRODUCT       | CREATE       | guid-1 (Administrator)
PRODUCT       | UPDATE       | guid-1 (Administrator)
PRODUCT       | DELETE       | guid-1 (Administrator)
PRODUCT       | VIEW         | guid-1 (Administrator)
PRODUCT       | IMPORT       | guid-1 (Administrator)
PRODUCT       | EXPORT       | guid-1 (Administrator)
PRODUCT       | CREATE       | guid-2 (Customer)
PRODUCT       | UPDATE       | guid-2 (Customer)
PRODUCT       | DELETE       | guid-2 (Customer)
PRODUCT       | VIEW         | guid-2 (Customer)
PRODUCT       | IMPORT       | guid-2 (Customer)
PRODUCT       | EXPORT       | guid-2 (Customer)
PRODUCT       | CREATE       | guid-3 (Agent)
PRODUCT       | UPDATE       | guid-3 (Agent)
...           | ...          | ...
-- Repeat for all 8 functions × 6 commands × 3 roles = 144 permissions
```

**Composite Primary Key:** `(Function, Command, RoleId)`

---

## ?? **SEED DATA SUMMARY**

### **Seeded on Startup:**
- ? **3 Roles** (Administrator, Customer, Agent)
- ? **3 Test Users** (one for each role)
- ? **144 Permissions** (48 per role × 3 roles)

### **Seed Users:**
```
admin@tedu.com.vn       ? Administrator ? Admin@123$    ? Full Access
customer@tedu.com.vn    ? Customer      ? Customer@123$ ? Full Access
agent@tedu.com.vn       ? Agent         ? Agent@123$    ? Full Access
```

---

## ?? **DEPLOYMENT NOTES**

### **Docker Container:**
```yaml
# docker-compose.override.yml
identity-api:
  environment:
    - "ConnectionStrings__IdentitySqlConnection=Server=identitydb,1433;Database=IdentityDb;..."
```

### **Auto-Migration:**
```csharp
// Program.cs
if (app.Environment.IsDevelopment())
{
    await SeedUserData.EnsureSeedDataAsync(connectionString);
}
```

**First Run:**
1. Start `identitydb` container
2. Start `identity-api` container
3. Database `IdentityDb` created automatically
4. Migrations applied
5. 3 Roles created
6. 3 Users seeded
7. 144 Permissions seeded

---

## ?? **TESTING**

### **Verify Seeded Data:**
```powershell
# Connect to database
docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -d IdentityDb

# Check roles (should return 3)
SELECT Name FROM AspNetRoles ORDER BY Name;
```

**Expected output:**
```
Name
------------------
Administrator
Agent
Customer

(3 rows affected)
```

```powershell
# Check users (should return 3)
SELECT UserName, Email FROM AspNetUsers ORDER BY UserName;
```

**Expected output:**
```
UserName                   Email
-------------------------- --------------------------
admin@tedu.com.vn         admin@tedu.com.vn
agent@tedu.com.vn         agent@tedu.com.vn
customer@tedu.com.vn      customer@tedu.com.vn

(3 rows affected)
```

```powershell
# Check permissions count (should return 144)
SELECT COUNT(*) as PermissionCount FROM Permissions;
```

**Expected output:**
```
PermissionCount
---------------
144

(1 row affected)
```

```powershell
# Check permissions by role
SELECT 
  r.Name as RoleName,
  COUNT(*) as PermissionCount
FROM Permissions p
JOIN AspNetRoles r ON p.RoleId = r.Id
GROUP BY r.Name
ORDER BY r.Name;
```

**Expected output:**
```
RoleName            PermissionCount
------------------- ---------------
Administrator       48
Agent               48
Customer            48

(3 rows affected)
```

### **Test Login:**
```powershell
# Test all 3 users
$users = @(
    @{ email="admin@tedu.com.vn"; password="Admin@123$"; role="Administrator" },
    @{ email="customer@tedu.com.vn"; password="Customer@123$"; role="Customer" },
    @{ email="agent@tedu.com.vn"; password="Agent@123$"; role="Agent" }
)

foreach ($user in $users) {
    Write-Host "`nTesting $($user.role)..." -ForegroundColor Yellow
    
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
            -ContentType "application/x-www-form-urlencoded"
        
        if ($response.access_token) {
            Write-Host "? $($user.role): Login successful" -ForegroundColor Green
            Write-Host "   Token: $($response.access_token.Substring(0, 50))..." -ForegroundColor Gray
        }
    } catch {
        Write-Host "? $($user.role): Login failed" -ForegroundColor Red
        Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}
```

---

## ?? **PRODUCTION CONSIDERATIONS**

### ?? **WARNING: This is a TESTING configuration!**

**For Production, you should:**

1. **Restrict Customer Permissions:**
```csharp
// Customer should only have:
PRODUCT: VIEW
ORDER: VIEW, CREATE (own orders only)
BASKET: CREATE, UPDATE, DELETE, VIEW
CUSTOMER: VIEW, UPDATE (own profile only)
```

2. **Restrict Agent Permissions:**
```csharp
// Agent should only have:
CUSTOMER: VIEW, UPDATE
ORDER: VIEW, CREATE, UPDATE
PRODUCT: VIEW
BASKET: VIEW, CREATE, UPDATE
SHIPPING: VIEW, UPDATE
```

3. **Keep Administrator Full Access:**
```csharp
// Administrator keeps all permissions
```

4. **Implement Resource-Level Authorization:**
```csharp
// Example: Customer can only view own orders
[HttpGet("{id}")]
[ClaimRequirement(FunctionCode.ORDER, CommandCode.VIEW)]
public async Task<IActionResult> GetOrder(Guid id)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var role = User.FindFirst(SystemConstants.Claims.Roles)?.Value;
    
    if (role == "Customer")
    {
        var order = await _orderService.GetOrderAsync(id);
        if (order.UserId != userId)
        {
            return Forbid(); // 403 - Not authorized to view this order
        }
    }
    
    return Ok(await _orderService.GetOrderAsync(id));
}
```

---

## ?? **QUICK RESET**

### **Reset Database and Reseed:**
```powershell
# Stop Identity API
docker-compose stop identity-api

# Remove database volume
docker volume rm backend_microservices_identity_data

# Restart containers
docker-compose up -d identitydb
Start-Sleep -Seconds 10
docker-compose up -d identity-api

# Wait for seeding
Start-Sleep -Seconds 30

# Verify
docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -d IdentityDb `
  -Q "SELECT Name FROM AspNetRoles; SELECT UserName FROM AspNetUsers; SELECT COUNT(*) as Permissions FROM Permissions;"
```

---

## ?? **SECURITY NOTES**

1. **Change Default Passwords** before production
2. **Disable full permissions** for non-admin roles in production
3. **Enable 2FA** for administrative roles
4. **Audit permission changes**
5. **Use HTTPS** in production
6. **Implement rate limiting** on token endpoint
7. **Add resource-level authorization** (own vs. all resources)

---

## ?? **SUMMARY**

? **3 Simple Roles**: Administrator, Customer, Agent  
? **Full Permissions**: All roles have 48 permissions each  
? **Easy Testing**: No permission restrictions  
? **3 Test Users**: One for each role  
? **Total Permissions**: 144 (48 × 3 roles)  
? **Database**: IdentityDb  
? **Auto-Seeded**: On first startup in Development

**Perfect for development and testing!** ??

---

**Last Updated:** 2025-01-20  
**Version:** 2.0.0 (Simplified)  
**Database:** IdentityDb  
**Configuration:** Testing/Development Only
