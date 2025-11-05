# Fix Folder Structure Issues - Docker & Paths
# Run this script from the repository root

Write-Host "?? Fixing folder structure issues..." -ForegroundColor Cyan

# 1. Update docker-compose.yml
Write-Host "1??  Updating docker-compose.yml..." -ForegroundColor Yellow

$dockerComposePath = "docker-compose.yml"
$content = Get-Content $dockerComposePath -Raw

$content = $content -replace "src/Services/Product\.API/Dockerfile", "src/Services/Product/Product.API/Dockerfile"
$content = $content -replace "src/Services/Customer\.API/Dockerfile", "src/Services/Customer/Customer.API/Dockerfile"
$content = $content -replace "src/Services/Basket\.API/Dockerfile", "src/Services/Basket/Basket.API/Dockerfile"
$content = $content -replace "src/Services/Hangfire\.API/Dockerfile", "src/Services/ScheduledJob/Hangfire.API/Dockerfile"

Set-Content $dockerComposePath $content
Write-Host "   ? docker-compose.yml updated" -ForegroundColor Green

# 2. Update Product.API Dockerfile
Write-Host "2??  Updating Product.API Dockerfile..." -ForegroundColor Yellow

$productDockerfile = "src/Services/Product/Product.API/Dockerfile"
if (Test-Path $productDockerfile) {
 $content = Get-Content $productDockerfile -Raw
    $content = $content -replace 'src/Services/Product\.API/', 'src/Services/Product/Product.API/'
    Set-Content $productDockerfile $content
    Write-Host "   ? Product.API Dockerfile updated" -ForegroundColor Green
}

# 3. Update Customer.API Dockerfile
Write-Host "3??  Updating Customer.API Dockerfile..." -ForegroundColor Yellow

$customerDockerfile = "src/Services/Customer/Customer.API/Dockerfile"
if (Test-Path $customerDockerfile) {
    $content = Get-Content $customerDockerfile -Raw
    $content = $content -replace 'src/Services/Customer\.API/', 'src/Services/Customer/Customer.API/'
    Set-Content $customerDockerfile $content
    Write-Host "   ? Customer.API Dockerfile updated" -ForegroundColor Green
}

# 4. Update Basket.API Dockerfile
Write-Host "4??  Updating Basket.API Dockerfile..." -ForegroundColor Yellow

$basketDockerfile = "src/Services/Basket/Basket.API/Dockerfile"
if (Test-Path $basketDockerfile) {
    $content = Get-Content $basketDockerfile -Raw
  $content = $content -replace 'src/Services/Basket\.API/', 'src/Services/Basket/Basket.API/'
    Set-Content $basketDockerfile $content
    Write-Host "   ? Basket.API Dockerfile updated" -ForegroundColor Green
}

# 5. Update Hangfire.API Dockerfile
Write-Host "5??  Updating Hangfire.API Dockerfile..." -ForegroundColor Yellow

$hangfireDockerfile = "src/Services/ScheduledJob/Hangfire.API/Dockerfile"
if (Test-Path $hangfireDockerfile) {
    $content = Get-Content $hangfireDockerfile -Raw
    $content = $content -replace 'src/Services/Hangfire\.API/', 'src/Services/ScheduledJob/Hangfire.API/'
    Set-Content $hangfireDockerfile $content
    Write-Host "   ? Hangfire.API Dockerfile updated" -ForegroundColor Green
}

# 6. Delete old duplicate Dockerfiles
Write-Host "6??  Removing old duplicate Dockerfiles..." -ForegroundColor Yellow

$oldDockerfiles = @(
    "src/Services/Customer.API/Dockerfile",
    "src/Services/Product.API/Dockerfile",
  "src/Services/Basket.API/Dockerfile",
    "src/Services/Hangfire.API/Dockerfile"
)

foreach ($file in $oldDockerfiles) {
    if (Test-Path $file) {
 Remove-Item $file -Force
      Write-Host "   ? Deleted: $file" -ForegroundColor Green
    }
}

# 7. Verify docker-compose.override.yml
Write-Host "7??  Checking docker-compose.override.yml..." -ForegroundColor Yellow

$overridePath = "docker-compose.override.yml"
if (Test-Path $overridePath) {
    Write-Host " ??  docker-compose.override.yml exists - manual check recommended" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "? All fixes completed!" -ForegroundColor Green
Write-Host ""
Write-Host "?? Next steps:" -ForegroundColor Cyan
Write-Host "   1. Run: docker-compose build" -ForegroundColor White
Write-Host "   2. Run: docker-compose up -d" -ForegroundColor White
Write-Host "   3. Verify all services start correctly" -ForegroundColor White
