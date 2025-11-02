# ?? Identity Database Migration Script

## **Quick Commands**

### **1. Clean Start (Delete & Recreate Database)**
```powershell
# Stop Identity API
docker-compose stop identity-api

# Remove identity database volume
docker volume rm backend_microservices_identity_data

# Restart containers
docker-compose up -d identitydb
Start-Sleep -Seconds 10
docker-compose up -d identity-api

# Monitor logs
docker-compose logs -f identity-api
```

### **2. Verify Database Created**
```powershell
docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -Q "SELECT name FROM sys.databases WHERE name='IdentityDb'"
```

**Expected output:**
```
name
--------
IdentityDb

(1 rows affected)
```

### **3. Verify Tables Created**
```sql
docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -d IdentityDb `
  -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME"
```

**Expected tables:**
```
AspNetRoles
AspNetRoleClaims
AspNetUsers
AspNetUserClaims
AspNetUserLogins
AspNetUserRoles
AspNetUserTokens
Clients
ClientSecrets
...
Permissions
```

### **4. Verify Roles Seeded**
```powershell
docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -d IdentityDb `
  -Q "SELECT Name FROM AspNetRoles ORDER BY Name"
```

**Expected output:**
```
Name
------------------
Accountant
Administrator
Customer
Developer
Manager
Marketing
SalesAgent
SupportAgent
WarehouseStaff

(9 rows affected)
```

### **5. Verify Users Seeded**
```powershell
docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -d IdentityDb `
  -Q "SELECT UserName, Email FROM AspNetUsers ORDER BY UserName"
```

**Expected output:**
```
UserName                   Email
-------------------------- --------------------------
admin@tedu.com.vn         admin@tedu.com.vn
customer@tedu.com.vn      customer@tedu.com.vn
manager@tedu.com.vn       manager@tedu.com.vn
sales@tedu.com.vn         sales@tedu.com.vn
support@tedu.com.vn       support@tedu.com.vn
warehouse@tedu.com.vn     warehouse@tedu.com.vn

(6 rows affected)
```

### **6. Verify Permissions Count**
```powershell
docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -d IdentityDb `
  -Q "SELECT COUNT(*) as PermissionCount FROM Permissions"
```

**Expected:** ~100-150 permissions depending on role configuration

### **7. Check Permissions by Role**
```powershell
docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -d IdentityDb `
  -Q "
  SELECT 
    r.Name as RoleName,
    COUNT(*) as PermissionCount
  FROM Permissions p
  JOIN AspNetRoles r ON p.RoleId = r.Id
  GROUP BY r.Name
  ORDER BY COUNT(*) DESC
  "
```

**Expected output:**
```
RoleName            PermissionCount
------------------- ---------------
Administrator       48
Manager             31
SalesAgent          15
Marketing           12
WarehouseStaff      11
Accountant          9
Customer            9
SupportAgent        8
Developer           5
```

### **8. View Sample Permissions for Administrator**
```powershell
docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -d IdentityDb `
  -Q "
  SELECT TOP 10
    p.Function,
    p.Command,
    r.Name as RoleName
  FROM Permissions p
  JOIN AspNetRoles r ON p.RoleId = r.Id
  WHERE r.Name = 'Administrator'
  ORDER BY p.Function, p.Command
  "
```

### **9. View Permissions for Customer Role**
```powershell
docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -d IdentityDb `
  -Q "
  SELECT 
    p.Function,
    p.Command
  FROM Permissions p
  JOIN AspNetRoles r ON p.RoleId = r.Id
  WHERE r.Name = 'Customer'
  ORDER BY p.Function, p.Command
  "
```

**Expected:**
```
Function    Command
----------- -----------
BASKET      CREATE
BASKET      DELETE
BASKET      UPDATE
BASKET      VIEW
CUSTOMER    UPDATE
CUSTOMER    VIEW
ORDER       CREATE
ORDER       VIEW
PRODUCT     VIEW
```

---

## ?? **Manual Migration Steps**

### **Step 1: Backup Existing Database (if any)**
```powershell
# Export data (if needed)
docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -d TeduIdentity `
  -Q "SELECT * FROM AspNetUsers" -o users_backup.txt

# Or use SQL Server Management Studio (SSMS)
# Connect to: localhost,1436
```

### **Step 2: Drop Old Database**
```sql
docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -Q "
  IF EXISTS (SELECT name FROM sys.databases WHERE name = 'TeduIdentity')
  BEGIN
    ALTER DATABASE TeduIdentity SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE TeduIdentity;
  END
  "
```

### **Step 3: Restart Identity API**
```powershell
# Identity API will auto-create IdentityDb and seed data
docker-compose restart identity-api

# Wait 30 seconds
Start-Sleep -Seconds 30

# Check logs
docker-compose logs identity-api | Select-String "IdentityDb"
```

### **Step 4: Verify New Database**
```powershell
# Check database exists
docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -Q "SELECT name FROM sys.databases WHERE name='IdentityDb'"

# Check tables
docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -d IdentityDb `
  -Q "SELECT COUNT(*) as TableCount FROM INFORMATION_SCHEMA.TABLES"
```

---

## ?? **Detailed Verification Queries**

### **1. User-Role Mapping**
```sql
SELECT 
  u.UserName,
  u.Email,
  r.Name as RoleName
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id
ORDER BY r.Name, u.UserName
```

### **2. User Claims**
```sql
SELECT 
  u.UserName,
  uc.ClaimType,
  uc.ClaimValue
FROM AspNetUsers u
JOIN AspNetUserClaims uc ON u.Id = uc.UserId
WHERE u.UserName = 'admin@tedu.com.vn'
ORDER BY uc.ClaimType
```

### **3. Permission Summary by Function**
```sql
SELECT 
  p.Function,
  COUNT(DISTINCT p.Command) as CommandCount,
  COUNT(DISTINCT r.Name) as RoleCount
FROM Permissions p
JOIN AspNetRoles r ON p.RoleId = r.Id
GROUP BY p.Function
ORDER BY p.Function
```

### **4. Roles with Full Access (All Commands)**
```sql
SELECT 
  r.Name as RoleName,
  p.Function,
  COUNT(*) as CommandCount
FROM Permissions p
JOIN AspNetRoles r ON p.RoleId = r.Id
GROUP BY r.Name, p.Function
HAVING COUNT(*) = 6  -- All 6 commands (CREATE, UPDATE, DELETE, VIEW, IMPORT, EXPORT)
ORDER BY r.Name, p.Function
```

### **5. Find Missing Permissions**
```sql
-- Expected: Administrator should have all permissions
DECLARE @AdminRoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Administrator')

SELECT 
  f.Function,
  c.Command
FROM 
  (SELECT 'PRODUCT' as Function UNION SELECT 'ORDER' UNION SELECT 'CUSTOMER' UNION 
   SELECT 'INVENTORY' UNION SELECT 'BASKET' UNION SELECT 'PAYMENT' UNION 
   SELECT 'SHIPPING' UNION SELECT 'REPORTING') f
CROSS JOIN
  (SELECT 'CREATE' as Command UNION SELECT 'UPDATE' UNION SELECT 'DELETE' UNION 
   SELECT 'VIEW' UNION SELECT 'IMPORT' UNION SELECT 'EXPORT') c
WHERE NOT EXISTS (
  SELECT 1 FROM Permissions p 
  WHERE p.Function = f.Function 
    AND p.Command = c.Command 
    AND p.RoleId = @AdminRoleId
)
```

---

## ?? **Test Scripts**

### **PowerShell Test Suite**

```powershell
# Test-IdentityDatabase.ps1

Write-Host "=== Identity Database Verification ===" -ForegroundColor Cyan

# 1. Database exists
Write-Host "`n1. Checking database..." -ForegroundColor Yellow
$dbCheck = docker exec identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -Q "SELECT name FROM sys.databases WHERE name='IdentityDb'" -h -1

if ($dbCheck -match "IdentityDb") {
    Write-Host "? Database 'IdentityDb' exists" -ForegroundColor Green
} else {
    Write-Host "? Database 'IdentityDb' not found" -ForegroundColor Red
    exit 1
}

# 2. Roles count
Write-Host "`n2. Checking roles..." -ForegroundColor Yellow
$roleCount = docker exec identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -d IdentityDb `
  -Q "SELECT COUNT(*) FROM AspNetRoles" -h -1

if ($roleCount -ge 9) {
    Write-Host "? Found $roleCount roles (expected 9+)" -ForegroundColor Green
} else {
    Write-Host "??  Only $roleCount roles found (expected 9)" -ForegroundColor Yellow
}

# 3. Users count
Write-Host "`n3. Checking users..." -ForegroundColor Yellow
$userCount = docker exec identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -d IdentityDb `
  -Q "SELECT COUNT(*) FROM AspNetUsers" -h -1

if ($userCount -ge 6) {
    Write-Host "? Found $userCount users (expected 6+)" -ForegroundColor Green
} else {
    Write-Host "??  Only $userCount users found (expected 6)" -ForegroundColor Yellow
}

# 4. Permissions count
Write-Host "`n4. Checking permissions..." -ForegroundColor Yellow
$permCount = docker exec identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -d IdentityDb `
  -Q "SELECT COUNT(*) FROM Permissions" -h -1

if ($permCount -ge 100) {
    Write-Host "? Found $permCount permissions" -ForegroundColor Green
} else {
    Write-Host "??  Only $permCount permissions found (expected 100+)" -ForegroundColor Yellow
}

# 5. Test login for each user
Write-Host "`n5. Testing user logins..." -ForegroundColor Yellow
$users = @(
    @{ email="admin@tedu.com.vn"; password="Admin@123$"; role="Administrator" },
    @{ email="manager@tedu.com.vn"; password="Manager@123$"; role="Manager" },
    @{ email="customer@tedu.com.vn"; password="Customer@123$"; role="Customer" },
    @{ email="sales@tedu.com.vn"; password="SalesAgent@123$"; role="SalesAgent" },
    @{ email="support@tedu.com.vn"; password="SupportAgent@123$"; role="SupportAgent" },
    @{ email="warehouse@tedu.com.vn"; password="Warehouse@123$"; role="WarehouseStaff" }
)

$successCount = 0
foreach ($user in $users) {
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
            Write-Host "  ? $($user.role): Login successful" -ForegroundColor Green
            $successCount++
        }
    } catch {
        Write-Host "  ? $($user.role): Login failed" -ForegroundColor Red
    }
}

Write-Host "`n=== Summary ===" -ForegroundColor Cyan
Write-Host "Successful logins: $successCount / $($users.Count)" -ForegroundColor $(if ($successCount -eq $users.Count) { "Green" } else { "Yellow" })

if ($successCount -eq $users.Count) {
    Write-Host "`n? All checks passed!" -ForegroundColor Green
    Write-Host "Identity database is correctly configured." -ForegroundColor Green
} else {
    Write-Host "`n??  Some checks failed" -ForegroundColor Yellow
    Write-Host "Please review the logs above." -ForegroundColor Yellow
}
```

**Run test:**
```powershell
.\Test-IdentityDatabase.ps1
```

---

## ?? **Troubleshooting**

### **Issue: Database not created**
```powershell
# Check Identity API logs
docker-compose logs identity-api | Select-String "error"

# Check connection string
docker exec identity-api printenv | grep IdentitySqlConnection

# Manually create database
docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -Q "CREATE DATABASE IdentityDb"
```

### **Issue: Migrations not applied**
```powershell
# Check if migrations exist
docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -d IdentityDb `
  -Q "SELECT * FROM __EFMigrationsHistory"

# Force re-run migrations
docker-compose down identity-api
docker-compose up -d identity-api
```

### **Issue: No seed data**
```powershell
# Check environment
docker exec identity-api printenv ASPNETCORE_ENVIRONMENT

# Should be "Development" for auto-seeding

# Check Program.cs logic
docker-compose logs identity-api | Select-String "SeedUserData"
```

### **Issue: Login fails**
```powershell
# Check user exists
docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -d IdentityDb `
  -Q "SELECT UserName, EmailConfirmed FROM AspNetUsers WHERE UserName='admin@tedu.com.vn'"

# Check password hash exists
docker exec -it identitydb /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "Marethyu2004!" `
  -d IdentityDb `
  -Q "SELECT UserName, SUBSTRING(PasswordHash, 1, 20) as PasswordHash FROM AspNetUsers WHERE UserName='admin@tedu.com.vn'"
```

---

## ?? **Notes**

- **Auto-migration runs on startup** if `ASPNETCORE_ENVIRONMENT=Development`
- **Seed data only runs** if tables are empty
- **Permissions are composite keys** (Function + Command + RoleId)
- **Use SSMS** for GUI management: `localhost,1436` / `sa` / `Marethyu2004!`

---

**Last Updated:** 2025-01-20  
**Database Name:** IdentityDb  
**Seed User Count:** 6  
**Seed Role Count:** 9  
**Approximate Permission Count:** 130+
